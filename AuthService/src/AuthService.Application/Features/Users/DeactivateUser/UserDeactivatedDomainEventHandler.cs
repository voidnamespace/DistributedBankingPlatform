using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Application.IntegrationEvents.Contracts;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Users.DeactivateUser;

public class UserDeactivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeactivatedDomainEvent>>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UserDeactivatedDomainEventHandler> _logger;

    public UserDeactivatedDomainEventHandler(
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UserDeactivatedDomainEventHandler> logger)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserDeactivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "UserDeactivatedDomainEvent received for user {UserId}",
            domainEvent.UserId);

        var integrationEvent = new UserDeactivatedIntegrationEvent(
            domainEvent.UserId);

        await _integrationEventPublisher.PublishAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserDeactivatedIntegrationEvent scheduled for publication for user {UserId}",
            domainEvent.UserId);
    }
}
