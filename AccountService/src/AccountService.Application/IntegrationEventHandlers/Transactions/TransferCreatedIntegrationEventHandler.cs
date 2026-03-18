using AccountService.Application.IntegrationEvents.Transactions;
using MediatR;
namespace AccountService.Application.IntegrationEventHandlers.Transactions;
    

public class TransferCreatedIntegrationEventHandler : INotificationHandler<TransferCreatedIntegrationEvent>
{

    private readonly IMediator _mediator;


    public TransferCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }



    public async Task Handle(TransferCreatedIntegrationEvent notification, CancellationToken ct)
    {

    }







}

