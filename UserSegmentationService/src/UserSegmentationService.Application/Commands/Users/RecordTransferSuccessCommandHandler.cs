using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Users;

public class RecordTransferSuccessCommandHandler
    : IRequestHandler<RecordTransferSuccessCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserMetricRepository _userMetricRepository;

    public RecordTransferSuccessCommandHandler(
        IUserAccountRepository userAccountRepository,
        IUserMetricRepository userMetricRepository)
    {
        _userAccountRepository = userAccountRepository;
        _userMetricRepository = userMetricRepository;
    }

    public async Task Handle(
        RecordTransferSuccessCommand request,
        CancellationToken cancellationToken)
    {
        var senderAccount = await _userAccountRepository.GetByAccountNumberAsync(
            request.FromAccountNumber,
            cancellationToken);

        if (senderAccount is null)
            return;

        var metric = await _userMetricRepository.GetByUserIdAsync(
            senderAccount.UserId,
            cancellationToken);

        if (metric is null)
        {
            metric = new UserMetric(senderAccount.UserId);
            _userMetricRepository.Add(metric);
        }

        metric.RecordSpend(request.Amount, request.RecordedAt);
    }
}
