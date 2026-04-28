using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserSegmentationService.Infrastructure.Persistence.Database;

public class SegmentationDbContextFactory
    : IDesignTimeDbContextFactory<SegmentationDbContext>
{
    public SegmentationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SegmentationDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5436;Database=segmentationdb;Username=postgres;Password=postgres"
            )
            .Options;

        return new SegmentationDbContext(options);
    }
}
