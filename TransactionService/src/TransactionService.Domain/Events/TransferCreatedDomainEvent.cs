using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Events;

public sealed record TransferCreatedDomainEvent(
    Guid InitiatorId,
    Guid TransactionId, 
    AccountNumberVO FromAccountNumber, 
    AccountNumberVO ToAccountNumber, 
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
