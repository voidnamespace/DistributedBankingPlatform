using BankCardService.Domain.Common;

namespace BankCardService.Domain.Events;

public sealed class BankCardActivatedEvent : IDomainEvent
{
    public Guid BankCardId { get; }
    public DateTime OccurredOn { get; }

    public BankCardActivatedEvent(Guid bankCardId, DateTime occurredOn)
    {
        BankCardId = bankCardId;
        OccurredOn = occurredOn;
    }
}
