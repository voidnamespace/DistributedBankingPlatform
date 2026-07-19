using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Application.IntegrationEvents.Contracts;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Users.DeleteUser;

public class UserDeletedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeletedDomainEvent>>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UserDeletedDomainEventHandler> _logger;

    public UserDeletedDomainEventHandler(
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UserDeletedDomainEventHandler> logger)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserDeletedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "UserDeletedDomainEvent received for user {UserId}",
            domainEvent.UserId);

        var integrationEvent = new UserDeletedIntegrationEvent(
            domainEvent.UserId);

        await _integrationEventPublisher.PublishAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserDeletedIntegrationEvent scheduled for publication for user {UserId}",
            domainEvent.UserId);
    }
}
