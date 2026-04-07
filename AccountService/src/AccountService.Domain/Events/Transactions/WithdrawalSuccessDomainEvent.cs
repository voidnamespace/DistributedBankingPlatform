using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events.Transactions;

public sealed record WithdrawalSuccessDomainEvent(
    Guid TransactionId,
    AccountNumberVO FromAccountNumber,
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
