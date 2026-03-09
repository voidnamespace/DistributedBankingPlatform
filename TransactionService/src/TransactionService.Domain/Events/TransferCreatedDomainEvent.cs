using TransactionService.Domain.ValueObjects;
namespace TransactionService.Domain.Events;

public sealed record TransferCreatedDomainEvent(Guid TransactionId, Guid FromAccountId, Guid ToAccountId, MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
