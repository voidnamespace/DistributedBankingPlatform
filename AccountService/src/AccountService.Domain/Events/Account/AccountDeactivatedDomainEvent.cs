namespace AccountService.Domain.Events.Account;

public sealed record AccountDeactivatedDomainEvent(
    Guid UserId,
    Guid AccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}