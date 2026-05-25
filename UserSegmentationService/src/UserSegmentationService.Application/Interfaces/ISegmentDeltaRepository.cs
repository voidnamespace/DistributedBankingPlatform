using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Interfaces;

public interface ISegmentDeltaRepository
{
    void Add(SegmentDelta delta);
}
