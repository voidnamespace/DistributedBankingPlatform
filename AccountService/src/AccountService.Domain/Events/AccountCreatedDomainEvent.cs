using AccountService.Domain.ValueObjects;
namespace AccountService.Domain.Events;

public sealed record AccountCreatedDomainEvent(
    Guid UserId,
    Guid AccountId, 
    AccountNumberVO AccountNumber,
    MoneyVO Balance,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
