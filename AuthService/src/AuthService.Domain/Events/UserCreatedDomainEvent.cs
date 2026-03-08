namespace AuthService.Domain.Events;

public sealed record UserCreatedDomainEvent(
    Guid UserId,
    string Email
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}