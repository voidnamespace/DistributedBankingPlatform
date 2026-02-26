using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;

public class UserActivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserActivatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public UserActivatedDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<UserActivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new UserActivatedIntegrationEvent(
            domainEvent.UserId);

        await _outbox.EnqueueAsync(integrationEvent, "user.activated", ct);
    }
}