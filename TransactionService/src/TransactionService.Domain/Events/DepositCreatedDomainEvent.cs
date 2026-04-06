using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Events;

public sealed record  DepositCreatedDomainEvent(
    Guid TransactionId, 
    AccountNumberVO ToAccountNumber, 
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
