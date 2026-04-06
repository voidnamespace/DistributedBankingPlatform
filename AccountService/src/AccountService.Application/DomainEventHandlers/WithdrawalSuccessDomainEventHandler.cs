using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Withdrawal;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class WithdrawalSuccessDomainEventHandler 
    : INotificationHandler<DomainEventNotification<WithdrawalSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public WithdrawalSuccessDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<WithdrawalSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new WithdrawalSuccessIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
