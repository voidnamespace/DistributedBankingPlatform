using MediatR;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Transfer;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class TransferCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<TransferCreatedDomainEvent>>
{

    private readonly IOutboxWriter _outbox;

    public TransferCreatedDomainEventHandler (IOutboxWriter outbox)
    {
        _outbox = outbox; 
    }

    public async Task Handle(
    DomainEventNotification<TransferCreatedDomainEvent> notification,
    CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new TransferCreatedIntegrationEvent(
            domainEvent.TransactionId,
            (string)domainEvent.FromAccountNumber.Value,
            (string)domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        await _outbox.EnqueueAsync(integrationEvent, ct);
    }


}
