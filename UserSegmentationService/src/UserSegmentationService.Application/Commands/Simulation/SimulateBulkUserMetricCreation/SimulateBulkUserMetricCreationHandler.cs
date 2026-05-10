using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricCreation;

public class SimulateBulkUserMetricCreationHandler : IRequestHandler<SimulateBulkUserMetricCreationCommand>
{
    private const int MetricsCount = 10_000;
    private const int ChunkSize = 1_000;

    private readonly IUserMetricRepository _userMetricRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SimulateBulkUserMetricCreationHandler(
        IUserMetricRepository userMetricRepository,
        IUnitOfWork unitOfWork)
    {
        _userMetricRepository = userMetricRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SimulateBulkUserMetricCreationCommand command,
        CancellationToken cancellationToken)
    {
        var random = Random.Shared;

        for (var created = 0; created < MetricsCount; created += ChunkSize)
        {
            var currentChunkSize = Math.Min(ChunkSize, MetricsCount - created);

            for (var i = 0; i < currentChunkSize; i++)
            {
                var userMetric = UserMetric.CreateSnapshot(
                    Guid.NewGuid(),
                    random.Next(0, 10_000),
                    null);

                _userMetricRepository.Add(userMetric);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
