using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Interfaces;

public interface ISegmentCache
{
    Task<Segment?> GetByRuleTypeAndKindAsync(
        SegmentRuleType ruleType,
        SegmentKind kind,
        CancellationToken cancellationToken = default);
}
