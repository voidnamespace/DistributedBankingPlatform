using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Common.Events;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.DomainEventHandlers;

public class UserDeletedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeletedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<UserDeletedDomainEventHandler> _logger;

    public UserDeletedDomainEventHandler(
        IOutboxWriter outbox,
        ILogger<UserDeletedDomainEventHandler> logger)
    {
        _outbox = outbox;
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

        await _outbox.EnqueueAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
            "UserDeletedIntegrationEvent enqueued to outbox for user {UserId}",
            domainEvent.UserId);
    }
}
