namespace AccountService.Domain.Events;

public sealed record AccountDeactivatedDomainEvent(
    Guid UserId,
    Guid AccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}