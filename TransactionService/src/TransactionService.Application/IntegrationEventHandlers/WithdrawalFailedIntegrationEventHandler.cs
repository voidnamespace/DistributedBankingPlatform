using MediatR;
using TransactionService.Application.Commands.MarkWithdrawalFailed;
using TransactionService.Application.IntegrationEvents.Withdrawal;

namespace TransactionService.Application.IntegrationEventHandlers;

public class WithdrawalFailedIntegrationEventHandler : INotificationHandler<WithdrawalFailedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public WithdrawalFailedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        WithdrawalFailedIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = new MarkWithdrawalFailedCommand(notification.TransactionId);
        await _mediator.Send(command, ct);  
    }
}
