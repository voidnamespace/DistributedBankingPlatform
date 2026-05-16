using UserSegmentationService.Domain.Events;
using UserSegmentationService.Domain.Exceptions;

namespace UserSegmentationService.Domain.Entities;

public class SegmentDelta : Entity
{
    private SegmentDelta()
    {
    }


    public static SegmentDelta Create(
        Guid id,
        Guid segmentId,
        IReadOnlyCollection<Guid> addedUserIds,
        IReadOnlyCollection<Guid> removedUserIds,
        DateTime createdAt)
    {
        var delta = new SegmentDelta(
            id,
            segmentId,
            addedUserIds,
            removedUserIds,
            createdAt);

        delta.AddDomainEvent(new SegmentDeltaCreatedDomainEvent(
            delta.Id,
            delta.SegmentId,
            delta.AddedUserIds,
            delta.RemovedUserIds,
            delta.CreatedAt));

        return delta;
    }

    private SegmentDelta(
        Guid id,
        Guid segmentId,
        IReadOnlyCollection<Guid> addedUserIds,
        IReadOnlyCollection<Guid> removedUserIds,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("Segment delta id cannot be empty.");

        if (segmentId == Guid.Empty)
            throw new DomainException("Segment id cannot be empty.");

        var distinctAddedUserIds = addedUserIds.Distinct().ToArray();
        var distinctRemovedUserIds = removedUserIds.Distinct().ToArray();

        if (distinctAddedUserIds.Length == 0 && distinctRemovedUserIds.Length == 0)
            throw new DomainException("Segment delta cannot be empty.");

        if (distinctAddedUserIds.Intersect(distinctRemovedUserIds).Any())
            throw new DomainException("Same user cannot be both added and removed in one segment delta.");

        Id = id;
        SegmentId = segmentId;
        AddedUserIds = addedUserIds.Distinct().ToArray();
        RemovedUserIds = removedUserIds.Distinct().ToArray();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid SegmentId { get; private set; }

    public Guid[] AddedUserIds { get; private set; } = Array.Empty<Guid>();

    public Guid[] RemovedUserIds { get; private set; } = Array.Empty<Guid>();

    public DateTime CreatedAt { get; private set; }
}
