using MediatR;
using AccountService.Domain.Events;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Transfer;

namespace AccountService.Application.DomainEventHandlers;

public class TransferSuccessDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public TransferSuccessDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<TransferSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new TransferSuccessIntegrationEvent(domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
