using AuthService.Application.Common.Events;
using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Features.Users.DeleteUser;
using AuthService.Application.IntegrationEvents.Contracts;
using AuthService.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Features.Users.DeleteUser;

public class UserDeletedDomainEventHandlerTests
{
    private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock = new();

    [Fact]
    public async Task Handle_WithValidDomainEvent_ShouldPublishIntegrationEventOnce()
    {
        // Arrange
        var domainEvent = new UserDeletedDomainEvent(Guid.NewGuid());
        var notification = new DomainEventNotification<UserDeletedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;
        UserDeletedIntegrationEvent? publishedEvent = null;

        _integrationEventPublisherMock
            .Setup(publisher => publisher.PublishAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken))
            .Callback<UserDeletedIntegrationEvent, CancellationToken>((integrationEvent, _) => publishedEvent = integrationEvent)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        _integrationEventPublisherMock.Verify(
            publisher => publisher.PublishAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken),
            Times.Once);

        publishedEvent.Should().NotBeNull();
        publishedEvent!.UserId.Should().Be(domainEvent.UserId);
    }

    [Fact]
    public async Task Handle_WhenPublisherThrows_ShouldPropagateException()
    {
        // Arrange
        var domainEvent = new UserDeletedDomainEvent(Guid.NewGuid());
        var notification = new DomainEventNotification<UserDeletedDomainEvent>(domainEvent);
        var cancellationToken = CancellationToken.None;

        _integrationEventPublisherMock
            .Setup(publisher => publisher.PublishAsync(It.IsAny<UserDeletedIntegrationEvent>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Publisher failure"));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(notification, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Publisher failure");
    }

    private UserDeletedDomainEventHandler CreateHandler()
    {
        return new UserDeletedDomainEventHandler(
            _integrationEventPublisherMock.Object,
            NullLogger<UserDeletedDomainEventHandler>.Instance);
    }
}
