namespace AccountService.Domain.Events.Account;

public sealed record  AccountActivatedDomainEvent(
    Guid UserId,
    Guid AccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
