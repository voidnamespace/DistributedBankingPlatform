namespace UserSegmentationService.Application.Interfaces;

public interface ISegmentMembershipRepository
{
    Task<IReadOnlyList<Guid>> GetUserIdsBySegmentIdAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default);

    Task ReplaceSegmentMembersAsync(
        Guid segmentId,
        IReadOnlyCollection<Guid> userIds,
        DateTime joinedAt,
        CancellationToken cancellationToken = default);
}
