using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class UserMetricRepository : IUserMetricRepository
{
    private readonly SegmentationDbContext _dbContext;

    public UserMetricRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserMetric?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserMetrics
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserMetrics
            .AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public void Add(UserMetric userMetric)
    {
        _dbContext.UserMetrics.Add(userMetric);
    }
}
