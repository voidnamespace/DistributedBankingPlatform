using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Events;

public sealed record WithdrawalCreatedDomainEvent(
    Guid TransactionId, 
    AccountNumberVO FromAccountNumber, 
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
