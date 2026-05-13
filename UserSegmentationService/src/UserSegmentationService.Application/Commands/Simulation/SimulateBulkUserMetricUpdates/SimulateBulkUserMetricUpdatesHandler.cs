using MediatR;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricUpdates;

public class SimulateBulkUserMetricUpdatesHandler
    : IRequestHandler<SimulateBulkUserMetricUpdatesCommand>
{
    private readonly IUserMetricRepository _userMetricRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SimulateBulkUserMetricUpdatesHandler(
        IUserMetricRepository userMetricRepository,
        IUnitOfWork unitOfWork)
    {
        _userMetricRepository = userMetricRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SimulateBulkUserMetricUpdatesCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Count <= 0)
            return;

        var userMetrics = await _userMetricRepository.GetRandomAsync(
            command.Count,
            cancellationToken);

        var now = DateTime.UtcNow;
        var random = Random.Shared;

        foreach (var userMetric in userMetrics)
        {
            var amount = random.Next(10, 500);
            userMetric.RecordSpend(amount, now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
