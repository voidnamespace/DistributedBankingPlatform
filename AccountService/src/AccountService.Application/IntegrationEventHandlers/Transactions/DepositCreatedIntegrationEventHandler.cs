

using AccountService.Application.Commands.DepositMoney;
using AccountService.Application.IntegrationEvents.Transactions.Deposit;
using MediatR;

namespace AccountService.Application.IntegrationEventHandlers.Transactions;

public class DepositCreatedIntegrationEventHandler : INotificationHandler<DepositCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public DepositCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        DepositCreatedIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = new DepositMoneyCommand(
            notification.TransactionId,
            notification.ToAccountNumber,
            notification.Amount,
            notification.Currency);

        await _mediator.Send(command, ct);





    }



}
