using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events.Transactions;

public sealed record TransferSuccessDomainEvent(
    Guid TransactionId, 
    AccountNumberVO FromAccountNumber, 
    AccountNumberVO ToAccountNumber, 
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
