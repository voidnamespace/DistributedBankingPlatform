using MediatR;
using TransactionService.Application.IntegrationEvents.Deposit;

namespace TransactionService.Application.IntegrationEventHandlers;

public class DepositSuccessIntegrationEventHandler : INotificationHandler<DepositSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;

    public DepositSuccessIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        DepositSuccessIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = 
    }

}
