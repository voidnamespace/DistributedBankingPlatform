using MediatR;
using AccountService.Domain.Events;
using AccountService.Application.IntegrationEvents;
using AccountService.Application.Interfaces.Messaging;

namespace AccountService.Application.EventHandlers;

public class TransferDoneDomainEventHandler
    : INotificationHandler<TransferDoneDomainEvent>
{
    private readonly IOutboxWriter _outbox;

    public TransferDoneDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        TransferDoneDomainEvent notification,
        CancellationToken ct)
    {
        var integrationEvent = new TransferCreatedIntegrationEvent(
            notification.FromAccountId,
            notification.ToAccountId,
            notification.Amount,
            notification.Currency
        );

        await _outbox.WriteAsync(integrationEvent, ct);
    }
}