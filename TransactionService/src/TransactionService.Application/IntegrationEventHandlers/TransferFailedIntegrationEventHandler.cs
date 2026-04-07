using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkTransactionFailed;
using TransactionService.Application.IntegrationEvents.Transfer;

namespace TransactionService.Application.IntegrationEventHandlers;

public class TransferFailedIntegrationEventHandler
    : INotificationHandler<TransferFailedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferFailedIntegrationEventHandler> _logger;

    public TransferFailedIntegrationEventHandler(
        IMediator mediator,
        ILogger<TransferFailedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        TransferFailedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "TransferFailedIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkTransactionFailedCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkTransactionFailedCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkTransactionFailedCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
