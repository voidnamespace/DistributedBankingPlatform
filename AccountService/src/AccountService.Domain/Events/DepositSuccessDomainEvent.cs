using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events;

public sealed record  DepositSuccessDomainEvent(
    Guid TransactionId,
    AccountNumberVO ToAccountNumber,
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
