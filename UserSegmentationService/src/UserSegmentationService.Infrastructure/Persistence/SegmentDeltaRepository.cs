using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class SegmentDeltaRepository : ISegmentDeltaRepository
{
    private readonly SegmentationDbContext _dbContext;

    public SegmentDeltaRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SegmentDelta delta,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SegmentDeltas.Add(delta);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
