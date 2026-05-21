using Microsoft.Extensions.Caching.Memory;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Infrastructure.Caching;

internal class SegmentCache : ISegmentCache
{
    private readonly IMemoryCache _cache;
    private readonly ISegmentRepository _segmentRepository;

    public SegmentCache(
        IMemoryCache cache,
        ISegmentRepository segmentRepository)
    {
        _cache = cache;
        _segmentRepository = segmentRepository;
    }

    public async Task<Segment?> GetByRuleTypeAndKindAsync(
        SegmentRuleType ruleType,
        SegmentKind kind,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"segments:{ruleType}:{kind}";

        if (_cache.TryGetValue(cacheKey, out Segment? segment))
            return segment;

        segment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            ruleType,
            kind,
            cancellationToken);

        if (segment is not null)
        {
            _cache.Set(
                cacheKey,
                segment,
                TimeSpan.FromHours(1));
        }

        return segment;
    }
}
