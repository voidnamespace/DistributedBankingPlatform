using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence.Seeding;

public static class SegmentsSeeder
{
    public static async Task SeedAsync(
        SegmentationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Segments.AnyAsync(cancellationToken))
            return;

        dbContext.Segments.AddRange(
            new Segment(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Active users",
                SegmentRuleType.ActiveUsers,
                SegmentKind.Dynamic),
            new Segment(
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                "VIP users",
                SegmentRuleType.VipUsers,
                SegmentKind.Dynamic),
            new Segment(
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                "Risk users",
                SegmentRuleType.RiskUsers,
                SegmentKind.Dynamic),
            new Segment(
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                "March campaign audience",
                SegmentRuleType.ActiveUsers,
                SegmentKind.Static));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
