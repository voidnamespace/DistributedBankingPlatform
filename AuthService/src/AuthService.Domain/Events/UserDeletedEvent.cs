namespace AuthService.Domain.Events;

public sealed record UserDeletedEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
