using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence.Repositories;

internal class SegmentDeltaRepository : ISegmentDeltaRepository
{
    private readonly SegmentationDbContext _dbContext;

    public SegmentDeltaRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(SegmentDelta delta)
    {
        _dbContext.SegmentDeltas.Add(delta);
    }

}
