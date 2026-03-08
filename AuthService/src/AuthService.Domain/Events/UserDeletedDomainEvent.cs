namespace AuthService.Domain.Events;

public sealed record UserDeletedDomainEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
