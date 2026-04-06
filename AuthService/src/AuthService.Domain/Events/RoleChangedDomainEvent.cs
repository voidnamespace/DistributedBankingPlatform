using AuthService.Domain.Enums;

namespace AuthService.Domain.Events;

public sealed record RoleChangedDomainEvent(
    Guid UserId,
    Roles OldRole,
    Roles NewRole) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
