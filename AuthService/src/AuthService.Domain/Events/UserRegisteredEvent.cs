namespace AuthService.Domain.Events;

public sealed record UserRegisteredEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
