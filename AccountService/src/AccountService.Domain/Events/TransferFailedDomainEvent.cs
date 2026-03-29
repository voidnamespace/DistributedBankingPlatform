using AccountService.Domain.Enums;
namespace AccountService.Domain.Events;

public sealed record TransferFailedDomainEvent(Guid TransactionId, string FromAccountNumber, string ToAccountNumber, decimal Amount, Currency Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
