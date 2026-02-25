using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;

public class UserDeletedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserDeletedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public UserDeletedDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<UserDeletedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new UserDeletedIntegrationEvent(
            domainEvent.UserId);

        await _outbox.EnqueueAsync(integrationEvent, "user.deleted", ct);
    }
}