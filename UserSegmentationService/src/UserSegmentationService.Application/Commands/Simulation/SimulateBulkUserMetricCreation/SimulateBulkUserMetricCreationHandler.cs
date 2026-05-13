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
                var lastTransactionAt = CreateRandomLastTransactionAt(random);

                var userMetric = UserMetric.CreateSnapshot(
                    Guid.NewGuid(),
                    random.Next(0, 10_000),
                    lastTransactionAt);

                _userMetricRepository.Add(userMetric);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static DateTime? CreateRandomLastTransactionAt(Random random)
    {
        var bucket = random.Next(0, 100);

        if (bucket < 10)
            return null;

        if (bucket < 70)
            return DateTime.UtcNow.AddDays(-random.Next(0, 30));

        return DateTime.UtcNow.AddDays(-random.Next(91, 180));
    }
}
