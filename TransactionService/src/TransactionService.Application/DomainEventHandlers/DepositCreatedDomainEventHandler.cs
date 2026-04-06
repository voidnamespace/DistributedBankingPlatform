using MediatR;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Deposit;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class DepositCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<DepositCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public DepositCreatedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle (
        DomainEventNotification<DepositCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new DepositCreatedIntegrationEvent(
            domainEvent.TransactionId,
            (string)domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

    }

}
