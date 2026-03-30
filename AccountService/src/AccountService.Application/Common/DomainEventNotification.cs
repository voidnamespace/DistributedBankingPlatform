using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.Common;

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
