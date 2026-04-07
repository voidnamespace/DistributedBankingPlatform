using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Commands.MarkDepositSuccess;
using TransactionService.Application.IntegrationEvents.Deposit;

namespace TransactionService.Application.IntegrationEventHandlers;

public class DepositSuccessIntegrationEventHandler
    : INotificationHandler<DepositSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepositSuccessIntegrationEventHandler> _logger;

    public DepositSuccessIntegrationEventHandler(
        IMediator mediator,
        ILogger<DepositSuccessIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        DepositSuccessIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "DepositSuccessIntegrationEvent received: TransactionId {TransactionId}",
            notification.TransactionId);

        var command = new MarkDepositSuccessCommand(
            notification.TransactionId);

        _logger.LogInformation(
            "MarkDepositSuccessCommand created: TransactionId {TransactionId}",
            notification.TransactionId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "MarkDepositSuccessCommand dispatched: TransactionId {TransactionId}",
            notification.TransactionId);
    }
}
