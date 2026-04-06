using MediatR;
using TransactionService.Application.Commands.MarkWithdrawalSuccess;
using TransactionService.Application.IntegrationEvents.Withdrawal;

namespace TransactionService.Application.IntegrationEventHandlers;

public class WithdrawalSuccessIntegrationEventHandler : INotificationHandler<WithdrawalSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;

    public WithdrawalSuccessIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle (
        WithdrawalSuccessIntegrationEvent notification, 
        CancellationToken ct)
    {
        var command = new MarkWithdrawalSuccessCommand(notification.TransactionId);

        await _mediator.Send(command, ct);
    }

}
