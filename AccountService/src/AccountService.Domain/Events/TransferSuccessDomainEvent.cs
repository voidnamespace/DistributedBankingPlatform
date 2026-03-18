using AccountService.Domain.Enums;
namespace AccountService.Domain.Events;

public sealed record TransferSuccessDomainEvent(Guid TransactionId, Guid FromAccountId, Guid ToAccountId, decimal Amount, Currency Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

