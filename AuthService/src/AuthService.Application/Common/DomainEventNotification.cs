using MediatR;
using AuthService.Domain.Events;

public class DomainEventNotification<TDomainEvent>
    : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}