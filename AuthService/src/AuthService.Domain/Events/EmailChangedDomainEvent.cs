using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Events;

public sealed record EmailChangedDomainEvent(
    Guid UserId,
    EmailVO OldEmail,
    EmailVO NewEmail
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
