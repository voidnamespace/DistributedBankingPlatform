using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Application.IntegrationEvents.Contracts;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Users.ActivateUser;

public class UserActivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserActivatedDomainEvent>>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UserActivatedDomainEventHandler> _logger;

    public UserActivatedDomainEventHandler(
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UserActivatedDomainEventHandler> logger)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserActivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "UserActivatedDomainEvent received for user {UserId}",
            domainEvent.UserId);

        var integrationEvent = new UserActivatedIntegrationEvent(
            domainEvent.UserId);

        await _integrationEventPublisher.PublishAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserActivatedIntegrationEvent scheduled for publication for user {UserId}",
            domainEvent.UserId);
    }
}
