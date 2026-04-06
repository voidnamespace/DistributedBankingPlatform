using AccountService.Application.Commands.WithdrawMoney;
using AccountService.Application.IntegrationEvents.Transactions.Withdrawal;
using MediatR;

namespace AccountService.Application.IntegrationEventHandlers.Transactions;

public class WithdrawalCreatedIntegrationEventHandler : INotificationHandler<WithdrawalCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;
    

    public WithdrawalCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        WithdrawalCreatedIntegrationEvent notification, 
        CancellationToken ct)
    {

        var command = new WithdrawMoneyCommand(
            notification.InitiatorId,
            notification.TransactionId,
            notification.FromAccountNumber,
            notification.Amount,
            notification.Currency);

        await _mediator.Send(command, ct);
        
    }



}
