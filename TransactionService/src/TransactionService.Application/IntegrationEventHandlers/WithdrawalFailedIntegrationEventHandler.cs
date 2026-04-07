using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkWithdrawalFailed;
using TransactionService.Application.IntegrationEvents.Withdrawal;

namespace TransactionService.Application.IntegrationEventHandlers;

public class WithdrawalFailedIntegrationEventHandler
    : INotificationHandler<WithdrawalFailedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<WithdrawalFailedIntegrationEventHandler> _logger;

    public WithdrawalFailedIntegrationEventHandler(
        IMediator mediator,
        ILogger<WithdrawalFailedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        WithdrawalFailedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawalFailedIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkWithdrawalFailedCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkWithdrawalFailedCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkWithdrawalFailedCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
