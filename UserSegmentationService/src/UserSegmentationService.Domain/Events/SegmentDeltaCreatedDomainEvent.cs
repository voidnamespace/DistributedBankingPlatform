namespace UserSegmentationService.Domain.Events;

public sealed record SegmentDeltaCreatedDomainEvent(
    Guid Id,
    Guid SegmentId,
    IReadOnlyCollection<Guid> AddedUserIds,
    IReadOnlyCollection<Guid> RemovedUserIds,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
