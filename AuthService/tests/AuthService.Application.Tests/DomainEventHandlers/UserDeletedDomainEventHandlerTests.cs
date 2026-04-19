using AuthService.Application.Common.Events;
using AuthService.Application.DomainEventHandlers;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.DomainEventHandlers;

public class UserDeletedDomainEventHandlerTests
{
    private readonly Mock<IOutboxWriter> _outboxWriterMock = new();

    [Fact]
    public async Task Handle_WithValidDomainEvent_ShouldEnqueueIntegrationEventOnce()
    {
        // Arrange
        var domainEvent = new UserDeletedDomainEvent(Guid.NewGuid());
        var notification = new DomainEventNotification<UserDeletedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;
        UserDeletedIntegrationEvent? enqueuedEvent = null;

        _outboxWriterMock
            .Setup(writer => writer.EnqueueAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken))
            .Callback<UserDeletedIntegrationEvent, CancellationToken>((integrationEvent, _) => enqueuedEvent = integrationEvent)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        _outboxWriterMock.Verify(
            writer => writer.EnqueueAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken),
            Times.Once);

        enqueuedEvent.Should().NotBeNull();
        enqueuedEvent!.UserId.Should().Be(domainEvent.UserId);
    }

    [Fact]
    public async Task Handle_WhenOutboxThrows_ShouldPropagateException()
    {
        // Arrange
        var domainEvent = new UserDeletedDomainEvent(Guid.NewGuid());
        var notification = new DomainEventNotification<UserDeletedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;

        _outboxWriterMock
            .Setup(writer => writer.EnqueueAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Outbox failure"));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(notification, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Outbox failure");
    }

    private UserDeletedDomainEventHandler CreateHandler()
    {
        return new UserDeletedDomainEventHandler(
            _outboxWriterMock.Object,
            NullLogger<UserDeletedDomainEventHandler>.Instance);
    }
}
