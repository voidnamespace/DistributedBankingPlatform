using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.EventHandlers;

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
            "UserCreatedDomainEvent received for user {UserId} {Email}",
            domainEvent.UserId,
            domainEvent.Email);

        var integrationEvent = new UserCreatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email);

        await _outbox.EnqueueAsync(
            integrationEvent,
            ct);
        _logger.LogInformation("type of integrationEvent = {Type}", integrationEvent.GetType().FullName);

        _logger.LogInformation(
            "UserCreatedIntegrationEvent enqueued to outbox for user {UserId}",
            domainEvent.UserId);
    }
}