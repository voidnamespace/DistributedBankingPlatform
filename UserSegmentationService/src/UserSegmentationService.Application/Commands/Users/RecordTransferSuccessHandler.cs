using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Users;

public class RecordTransferSuccessHandler
    : IRequestHandler<RecordTransferSuccessCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserMetricRepository _userMetricRepository;


    public RecordTransferSuccessHandler(
        IUserAccountRepository userAccountRepository,
        IUserMetricRepository userMetricRepository)
    {
        _userAccountRepository = userAccountRepository;
        _userMetricRepository = userMetricRepository;

    }

    public async Task Handle(
        RecordTransferSuccessCommand command,
        CancellationToken cancellationToken)
    {
        var senderAccount = await _userAccountRepository.GetByAccountNumberAsync(
            command.FromAccountNumber,
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

        metric.RecordSpend(command.Amount, command.RecordedAt);

    }
}
