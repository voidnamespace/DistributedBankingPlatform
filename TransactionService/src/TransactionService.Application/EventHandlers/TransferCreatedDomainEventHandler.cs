using MediatR;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;
namespace TransactionService.Application.EventHandlers;

public class TransferCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<TransferCreatedDomainEvent>>
{

    private readonly IOutboxWriter _outbox;

    public TransferCreatedDomainEventHandler (IOutboxWriter outbox)
    {
        _outbox = outbox; 
    }

    public async Task Handle(DomainEventNotification<TransferCreatedDomainEvent> notification, CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var amount = domainEvent.Money.Amount;
        var currency = domainEvent.Money.Currency;


        var integrationEvent = new TransferCreatedIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.FromAccountId,
            domainEvent.ToAccountId,
            amount,
            currency
            );


    }



}
