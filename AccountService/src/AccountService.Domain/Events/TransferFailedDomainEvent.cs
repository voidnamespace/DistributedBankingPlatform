using AccountService.Domain.ValueObjects;

namespace AccountService.Domain.Events;

public sealed record TransferFailedDomainEvent(
    Guid TransactionId, 
    AccountNumberVO FromAccountNumber, 
    AccountNumberVO ToAccountNumber,
    MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
