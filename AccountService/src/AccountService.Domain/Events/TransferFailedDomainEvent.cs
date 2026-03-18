
using AccountService.Domain.Enums;

namespace AccountService.Domain.Events;

public sealed record TransferFailedDomainEvent(Guid FromAccountId, Guid ToAccountId, decimal Amount, Currency Currency, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

