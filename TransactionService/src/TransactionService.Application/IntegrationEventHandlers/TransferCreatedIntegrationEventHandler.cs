using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.Application.IntegrationEvents;

namespace TransactionService.Application.IntegrationEventHandlers;

public class TransferCreatedIntegrationEventHandler : INotificationHandler<TransferCreatedIntegrationEventHandler>
{

    private readonly IMediator _mediator;
    private readonly ILogger<TransferCreatedIntegrationEventHandler> _logger;

    public TransferCreatedIntegrationEventHandler(IMediator mediator,
        ILogger<TransferCreatedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }


    public async Task Handle(TransferCreatedIntegrationEvent notification,
        CancellationToken ct)
    {

        







    }





}
