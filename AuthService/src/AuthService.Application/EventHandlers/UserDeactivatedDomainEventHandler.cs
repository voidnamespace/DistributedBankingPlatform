using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;
using AuthService.Application.Common.Events;
namespace AuthService.Application.EventHandlers;

public class UserDeactivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeactivatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public UserDeactivatedDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<UserDeactivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new UserDeactivatedIntegrationEvent(
            domainEvent.UserId);

        await _outbox.EnqueueAsync(integrationEvent, "user.deactivated", ct);
    }
}