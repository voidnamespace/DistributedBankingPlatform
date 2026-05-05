using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence.Seeding;

public static class UserMetricsSeeder
{
    public static async Task SeedAsync(
        SegmentationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.UserMetrics.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        dbContext.UserMetrics.AddRange(
            UserMetric.CreateSnapshot(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                750m,
                now.AddDays(-10)),
            UserMetric.CreateSnapshot(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                320m,
                now.AddDays(-28)),
            UserMetric.CreateSnapshot(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                140m,
                now.AddDays(-45)),
            UserMetric.CreateSnapshot(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                0m,
                now.AddDays(-70)),
            UserMetric.CreateSnapshot(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                0m,
                now.AddDays(-120)),
            UserMetric.CreateSnapshot(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                0m,
                null));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
