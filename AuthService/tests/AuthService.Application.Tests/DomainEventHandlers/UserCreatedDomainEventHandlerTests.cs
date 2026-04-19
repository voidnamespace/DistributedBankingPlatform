using AuthService.Application.Common.Events;
using AuthService.Application.DomainEventHandlers;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Domain.Events;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.DomainEventHandlers;

public class UserCreatedDomainEventHandlerTests
{
    private readonly Mock<IOutboxWriter> _outboxWriterMock = new();

    [Fact]
    public async Task Handle_WithValidDomainEvent_ShouldEnqueueIntegrationEventOnce()
    {
        // Arrange
        var domainEvent = new UserCreatedDomainEvent(
            Guid.NewGuid(),
            new EmailVO("alice@example.com"));
        var notification = new DomainEventNotification<UserCreatedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;
        UserCreatedIntegrationEvent? enqueuedEvent = null;

        _outboxWriterMock
            .Setup(writer => writer.EnqueueAsync(It.IsAny<UserCreatedIntegrationEvent>(), cancellationToken))
            .Callback<UserCreatedIntegrationEvent, CancellationToken>((integrationEvent, _) => enqueuedEvent = integrationEvent)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        _outboxWriterMock.Verify(
            writer => writer.EnqueueAsync(It.IsAny<UserCreatedIntegrationEvent>(), cancellationToken),
            Times.Once);

        enqueuedEvent.Should().NotBeNull();
        enqueuedEvent!.UserId.Should().Be(domainEvent.UserId);
        enqueuedEvent.Email.Should().Be(domainEvent.Email.Value);
    }

    [Fact]
    public async Task Handle_WhenOutboxThrows_ShouldPropagateException()
    {
        // Arrange
        var domainEvent = new UserCreatedDomainEvent(
            Guid.NewGuid(),
            new EmailVO("alice@example.com"));
        var notification = new DomainEventNotification<UserCreatedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;

        _outboxWriterMock
            .Setup(writer => writer.EnqueueAsync(It.IsAny<UserCreatedIntegrationEvent>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Outbox failure"));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(notification, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Outbox failure");
    }

    private UserCreatedDomainEventHandler CreateHandler()
    {
        return new UserCreatedDomainEventHandler(
            _outboxWriterMock.Object,
            NullLogger<UserCreatedDomainEventHandler>.Instance);
    }
}
