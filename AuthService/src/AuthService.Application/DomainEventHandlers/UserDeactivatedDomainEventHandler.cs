using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Common.Events;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.DomainEventHandlers;

public class UserDeactivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeactivatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<UserDeactivatedDomainEventHandler> _logger;

    public UserDeactivatedDomainEventHandler(
        IOutboxWriter outbox,
        ILogger<UserDeactivatedDomainEventHandler> logger)
    {
        _outbox = outbox;
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

        await _outbox.EnqueueAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserDeactivatedIntegrationEvent enqueued to outbox for user {UserId}",
            domainEvent.UserId);
    }
}