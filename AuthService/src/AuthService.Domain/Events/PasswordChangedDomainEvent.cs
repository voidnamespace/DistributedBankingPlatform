namespace AuthService.Domain.Events;

public sealed record PasswordChangedDomainEvent(
    Guid UserId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
