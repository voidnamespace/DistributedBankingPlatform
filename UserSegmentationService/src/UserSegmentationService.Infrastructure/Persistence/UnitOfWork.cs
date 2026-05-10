using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly SegmentationDbContext _dbContext;

    public UnitOfWork(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
