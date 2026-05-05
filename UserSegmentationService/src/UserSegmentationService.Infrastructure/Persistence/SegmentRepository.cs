using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class SegmentRepository : ISegmentRepository
{
    private readonly SegmentationDbContext _dbContext;

    public SegmentRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Segment?> GetByRuleTypeAndKindAsync(
        SegmentRuleType ruleType,
        SegmentKind kind,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Segments
            .FirstOrDefaultAsync(
                x => x.RuleType == ruleType && x.Kind == kind,
                cancellationToken);
    }
}
