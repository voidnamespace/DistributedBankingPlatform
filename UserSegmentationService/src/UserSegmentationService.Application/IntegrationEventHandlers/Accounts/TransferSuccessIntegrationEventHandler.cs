using MediatR;
using UserSegmentationService.Application.Commands.Users;
using UserSegmentationService.Application.IntegrationEvents.Accounts;

namespace UserSegmentationService.Application.IntegrationEventHandlers.Accounts;

public class TransferSuccessIntegrationEventHandler
    : INotificationHandler<TransferSuccessIntegrationEvent>
{
    private readonly IMediator _mediator;

    public TransferSuccessIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        TransferSuccessIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RecordTransferSuccessCommand(
                notification.TransactionId,
                notification.FromAccountNumber,
                notification.ToAccountNumber,
                notification.Amount,
                notification.Currency,
                DateTime.UtcNow),
            cancellationToken);
    }
}
