namespace AccountService.Domain.Events;

public sealed record AccounDeactivatedDomainEvent(
    Guid UserId,
    Guid AccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}