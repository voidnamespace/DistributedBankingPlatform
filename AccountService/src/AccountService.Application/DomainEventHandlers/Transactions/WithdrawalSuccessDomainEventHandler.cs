using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Withdrawal;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Transactions;

public class WithdrawalSuccessDomainEventHandler
    : INotificationHandler<DomainEventNotification<WithdrawalSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<WithdrawalSuccessDomainEventHandler> _logger;

    public WithdrawalSuccessDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<WithdrawalSuccessDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<WithdrawalSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "WithdrawalSuccessDomainEvent received for transaction {TransactionId}, account {AccountNumber}, amount {Amount} {Currency}",
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.Money.Amount,
            domainEvent.Money.Currency);

        var integrationEvent = new WithdrawalSuccessIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "WithdrawalSuccessIntegrationEvent queued for transaction {TransactionId}",
            domainEvent.TransactionId);
    }
}
