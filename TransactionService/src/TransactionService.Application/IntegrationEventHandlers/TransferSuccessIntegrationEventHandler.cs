using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkTransactionSuccess;
using TransactionService.Application.IntegrationEvents.Transfer;

namespace TransactionService.Application.IntegrationEventHandlers;

public class TransferSuccessIntegrationEventHandler
    : INotificationHandler<TransferSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferSuccessIntegrationEventHandler> _logger;

    public TransferSuccessIntegrationEventHandler(
        IMediator mediator,
        ILogger<TransferSuccessIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        TransferSuccessIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "TransferSuccessIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkTransactionSuccessCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkTransactionSuccessCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkTransactionSuccessCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
