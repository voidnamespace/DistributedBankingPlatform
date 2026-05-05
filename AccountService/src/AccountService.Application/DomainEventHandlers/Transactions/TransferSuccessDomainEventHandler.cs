using MediatR;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using Microsoft.Extensions.Logging;
using AccountService.Domain.Events.Transactions;

namespace AccountService.Application.DomainEventHandlers.Transactions;

public class TransferSuccessDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<TransferSuccessDomainEventHandler> _logger;

    public TransferSuccessDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<TransferSuccessDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<TransferSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "TransferSuccessDomainEvent received for transaction {TransactionId}, from {FromAccountNumber} to {ToAccountNumber}, amount {Amount} {Currency}",
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            domainEvent.Money.Currency);

        var integrationEvent = new TransferSuccessIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency,
            domainEvent.OccurredOn
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "TransferSuccessIntegrationEvent queued for transaction {TransactionId}",
            domainEvent.TransactionId);
    }
}
