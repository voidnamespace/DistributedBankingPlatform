using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence.Repositories;

internal class UserMetricRepository : IUserMetricRepository
{
    private readonly SegmentationDbContext _dbContext;

    public UserMetricRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserMetric?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserMetrics
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserMetrics
            .AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync(
        DateTime activeSince,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserMetrics
            .Where(x => x.LastTransactionAt != null)
            .Where(x => x.LastTransactionAt >= activeSince)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetVipUserIdsAsync(
        decimal minimumSpend,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserMetrics
            .Where(x => x.SpendLast60Days.Amount >= minimumSpend)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetRiskUserIdsAsync(
        DateTime inactiveSince,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserMetrics
            .Where(x => x.LastTransactionAt == null || x.LastTransactionAt < inactiveSince)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserMetric>> GetRandomAsync(
        int count,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserMetrics
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public void Add(UserMetric userMetric)
    {
        _dbContext.UserMetrics.Add(userMetric);
    }
}
