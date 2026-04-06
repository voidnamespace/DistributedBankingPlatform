using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(
        IOutboxWriter outbox,
        ILogger<UserCreatedDomainEventHandler> logger)
    {
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
             "Handling UserCreatedDomainEvent for user {UserId} with email {Email}",
             domainEvent.UserId,
             domainEvent.Email);

        var integrationEvent = new UserCreatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email.Value);

        _logger.LogInformation(
          "IntegrationEvent type resolved: {IntegrationEventType}",
           integrationEvent.GetType().FullName);

        await _outbox.EnqueueAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
         "Outbox enqueue: UserCreatedIntegrationEvent for user {UserId}",
         domainEvent.UserId);
    }
}
