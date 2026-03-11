using AccountService.Domain.Enums;
namespace AccountService.Domain.Events;

public sealed record TransferDoneDomainEvent(Guid FromAccountId, Guid ToAccountId, decimal Amount, Currency Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

