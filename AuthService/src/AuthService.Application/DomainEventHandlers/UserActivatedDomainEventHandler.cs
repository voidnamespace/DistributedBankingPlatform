using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Common.Events;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.DomainEventHandlers;

public class UserActivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserActivatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<UserActivatedDomainEventHandler> _logger;

    public UserActivatedDomainEventHandler(
        IOutboxWriter outbox,
        ILogger<UserActivatedDomainEventHandler> logger)
    {
        _outbox = outbox;
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

        await _outbox.EnqueueAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserActivatedIntegrationEvent enqueued to outbox for user {UserId}",
            domainEvent.UserId);
    }
}
