using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;

public class UserCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public UserCreatedDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<UserCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new UserCreatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email);

        await _outbox.EnqueueAsync(integrationEvent, "user.created", ct);
    }
}