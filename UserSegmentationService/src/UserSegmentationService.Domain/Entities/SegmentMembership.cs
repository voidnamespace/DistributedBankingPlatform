namespace UserSegmentationService.Domain.Entities;

public class SegmentMembership
{
    private SegmentMembership()
    {
    }

    public SegmentMembership(
        Guid segmentId,
        Guid userId,
        DateTime joinedAt)
    {
        if (segmentId == Guid.Empty)
            throw new ArgumentException("Segment id cannot be empty.", nameof(segmentId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        SegmentId = segmentId;
        UserId = userId;
        JoinedAt = joinedAt;
    }

    public Guid SegmentId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime JoinedAt { get; private set; }
}
