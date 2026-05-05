using Microsoft.EntityFrameworkCore;

namespace UserSegmentationService.Infrastructure.Persistence.Database;

public class DatabaseMigrator
{
    private readonly SegmentationDbContext _dbContext;

    public DatabaseMigrator(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.MigrateAsync(cancellationToken);
    }
}
