using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events.Balance;

public sealed record BalanceChangedDomainEvent(
    Guid UserId,
    Guid AccountId,
    MoneyVO BalanceOld,
    MoneyVO BalanceNew) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
