using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Deposit;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Transactions;

public class DepositSuccessDomainEventHandler
    : INotificationHandler<DomainEventNotification<DepositSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<DepositSuccessDomainEventHandler> _logger;

    public DepositSuccessDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<DepositSuccessDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<DepositSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "DepositSuccessDomainEvent received for transaction {TransactionId}, account {AccountNumber}, amount {Amount} {Currency}",
            domainEvent.TransactionId,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            domainEvent.Money.Currency);

        var integrationEvent = new DepositSuccessIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "DepositSuccessIntegrationEvent queued for transaction {TransactionId}",
            domainEvent.TransactionId);
    }
}
