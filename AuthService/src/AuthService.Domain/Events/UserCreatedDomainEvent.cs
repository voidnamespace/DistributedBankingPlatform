using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Events;

public sealed record UserCreatedDomainEvent(
    Guid UserId,
    EmailVO Email
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

