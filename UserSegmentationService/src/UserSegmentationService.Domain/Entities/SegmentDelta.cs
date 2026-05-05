namespace UserSegmentationService.Domain.Entities;

public class SegmentDelta
{
    private SegmentDelta()
    {
    }

    public SegmentDelta(
        Guid id,
        Guid segmentId,
        IReadOnlyCollection<Guid> addedUserIds,
        IReadOnlyCollection<Guid> removedUserIds,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Segment delta id cannot be empty.", nameof(id));

        if (segmentId == Guid.Empty)
            throw new ArgumentException("Segment id cannot be empty.", nameof(segmentId));

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
