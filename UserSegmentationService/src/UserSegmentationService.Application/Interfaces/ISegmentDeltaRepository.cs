using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Interfaces;

public interface ISegmentDeltaRepository
{
    Task AddAsync(
        SegmentDelta delta,
        CancellationToken cancellationToken = default);
}
