using MediatR;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Application.Commands.Simulation.LastTransactionAt;

public class SimulateLastTransactionAtHandler
    : IRequestHandler<SimulateLastTransactionAtCommand>
{
    private readonly IUserMetricRepository _userMetricRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SimulateLastTransactionAtHandler(
        IUserMetricRepository userMetricRepository,
        IUnitOfWork unitOfWork)
    {
        _userMetricRepository = userMetricRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SimulateLastTransactionAtCommand command,
        CancellationToken ct)
    {
        var user = await _userMetricRepository.GetByUserIdAsync(
            command.UserId,
            ct);

        if (user == null)
            return;

        var now = DateTime.UtcNow;
        user.RecordTransaction(now);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
