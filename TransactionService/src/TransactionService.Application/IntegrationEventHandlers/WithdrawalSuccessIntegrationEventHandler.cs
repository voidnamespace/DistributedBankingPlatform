using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkWithdrawalSuccess;
using TransactionService.Application.IntegrationEvents.Withdrawal;

namespace TransactionService.Application.IntegrationEventHandlers;

public class WithdrawalSuccessIntegrationEventHandler
    : INotificationHandler<WithdrawalSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<WithdrawalSuccessIntegrationEventHandler> _logger;

    public WithdrawalSuccessIntegrationEventHandler(
        IMediator mediator,
        ILogger<WithdrawalSuccessIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        WithdrawalSuccessIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawalSuccessIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkWithdrawalSuccessCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkWithdrawalSuccessCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkWithdrawalSuccessCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
