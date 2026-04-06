using MediatR;
using TransactionService.Application.Commands.MarkDepositFailed;
using TransactionService.Application.IntegrationEvents.Deposit;

namespace TransactionService.Application.IntegrationEventHandlers;

public class DepositFailedIntegrationEventHandler : INotificationHandler<DepositFailedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public DepositFailedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        DepositFailedIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = new MarkDepositFailedCommand(notification.TransactionId);

        await _mediator.Send(command, ct);
    }

}
