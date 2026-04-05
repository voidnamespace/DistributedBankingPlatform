using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Deposit;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class DepositSuccessDomainEventHandler : INotificationHandler<DomainEventNotification<DepositSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public DepositSuccessDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<DepositSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new DepositSuccessIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

    }
}
