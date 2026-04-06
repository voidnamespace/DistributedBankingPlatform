using MediatR;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Withdrawal;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class WithdrawalCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<WithdrawalCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public WithdrawalCreatedDomainEventHandler(IOutboxWriter outboxWrier)
    {
        _outboxWriter = outboxWrier;
    }

    public async Task Handle(
        DomainEventNotification<WithdrawalCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new WithdrawalCreatedIntegrationEvent(
            domainEvent.InitiatorId,
            domainEvent.TransactionId,
            (string)domainEvent.FromAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

    }

}
