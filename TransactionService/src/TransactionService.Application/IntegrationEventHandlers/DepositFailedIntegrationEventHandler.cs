using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkDepositFailed;
using TransactionService.Application.IntegrationEvents.Deposit;

namespace TransactionService.Application.IntegrationEventHandlers;

public class DepositFailedIntegrationEventHandler
    : INotificationHandler<DepositFailedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepositFailedIntegrationEventHandler> _logger;

    public DepositFailedIntegrationEventHandler(
        IMediator mediator,
        ILogger<DepositFailedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        DepositFailedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "DepositFailedIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkDepositFailedCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkDepositFailedCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkDepositFailedCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
