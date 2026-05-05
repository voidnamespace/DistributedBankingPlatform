namespace UserSegmentationService.Application.Interfaces;

public interface ISegmentMembershipRepository
{
    Task ReplaceSegmentMembersAsync(
        Guid segmentId,
        IReadOnlyCollection<Guid> userIds,
        DateTime joinedAt,
        CancellationToken cancellationToken = default);
}
