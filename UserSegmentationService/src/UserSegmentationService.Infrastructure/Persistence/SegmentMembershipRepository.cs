using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class SegmentMembershipRepository : ISegmentMembershipRepository
{
    private readonly SegmentationDbContext _dbContext;

    public SegmentMembershipRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsBySegmentIdAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SegmentMemberships
            .Where(x => x.SegmentId == segmentId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceSegmentMembersAsync(
        Guid segmentId,
        IReadOnlyCollection<Guid> userIds,
        DateTime joinedAt,
        CancellationToken cancellationToken = default)
    {
        var currentMembers = await _dbContext.SegmentMemberships
            .Where(x => x.SegmentId == segmentId)
            .ToListAsync(cancellationToken);

        _dbContext.SegmentMemberships.RemoveRange(currentMembers);

        var newMembers = userIds
            .Distinct()
            .Select(userId => new SegmentMembership(
                segmentId,
                userId,
                joinedAt));

        _dbContext.SegmentMemberships.AddRange(newMembers);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
