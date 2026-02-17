using BankCardService.Domain.Common;

namespace BankCardService.Domain.Events;

public sealed class BankCardDeactivatedEvent : IDomainEvent
{
    public Guid BankCardId { get; }
    public DateTime OccurredOn { get; }

    public BankCardDeactivatedEvent(Guid bankCardId, DateTime occurredOn)
    {
        BankCardId = bankCardId;
        OccurredOn = occurredOn;
    }
}
