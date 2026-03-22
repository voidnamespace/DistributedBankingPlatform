using MediatR;
using TransactionService.Domain.Events;
namespace TransactionService.Application.Common;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent  // impressive, explain?
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
