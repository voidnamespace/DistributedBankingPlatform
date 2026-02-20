using MediatR;
using AuthService.Domain.Events;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.IntegrationEvents;

public class UserCreatedDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<UserCreatedDomainEvent>>
{
    private readonly IEventPublisher _eventPublisher;

    public UserCreatedDomainEventHandler(
        IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(
        DomainEventNotification<UserCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        await _eventPublisher.PublishAsync(
            new UserCreatedIntegrationEvent(
                domainEvent.UserId,
                domainEvent.Email),
            "user.created",
            ct);
    }
}