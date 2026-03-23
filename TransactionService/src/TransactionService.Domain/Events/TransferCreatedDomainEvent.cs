using TransactionService.Domain.ValueObjects;
namespace TransactionService.Domain.Events;

public sealed record TransferCreatedDomainEvent(Guid TransactionId, string FromAccountNumber, string ToAccountNumber, MoneyVO Money) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
