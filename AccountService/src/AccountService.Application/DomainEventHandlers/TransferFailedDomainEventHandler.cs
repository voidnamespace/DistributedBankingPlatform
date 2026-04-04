using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class TransferFailedDomainEventHandler : INotificationHandler<DomainEventNotification<TransferFailedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public TransferFailedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
      DomainEventNotification<TransferFailedDomainEvent> notification,
      CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new TransferFailedIntegrationEvent(domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
