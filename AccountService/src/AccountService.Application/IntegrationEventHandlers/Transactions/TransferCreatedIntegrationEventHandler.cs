using AccountService.Application.Commands.TransferMoney;
using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.IntegrationEventHandlers.Transactions;
    

public class TransferCreatedIntegrationEventHandler : INotificationHandler<TransferCreatedIntegrationEvent>
{

    private readonly IMediator _mediator;
    private readonly ILogger<TransferCreatedIntegrationEventHandler> _logger;

    public TransferCreatedIntegrationEventHandler(IMediator mediator,
        ILogger<TransferCreatedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(TransferCreatedIntegrationEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "TransferCreatedIntegrationEvent received {TransactionId}",
            notification.TransactionId);

        var command = new TransferMoneyCommand(notification.TransactionId,
            notification.FromAccountNumber, 
            notification.ToAccountNumber, 
            notification.Amount, 
            notification.Currency);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "TransferCreatedIntegrationEvent sent {TransactionId}",
            notification.TransactionId);

    }
}
