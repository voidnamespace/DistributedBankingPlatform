using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events;

public sealed record WithdrawalSuccessDomainEvent(
    Guid TransactionId,
    AccountNumberVO FromAccountNumber,
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
