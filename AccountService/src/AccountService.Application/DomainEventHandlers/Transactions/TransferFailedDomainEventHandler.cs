using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Transactions;

public class TransferFailedDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferFailedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<TransferFailedDomainEventHandler> _logger;

    public TransferFailedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<TransferFailedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<TransferFailedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "TransferFailedDomainEvent received for transaction {TransactionId}, from {FromAccountNumber} to {ToAccountNumber}, amount {Amount} {Currency}, reason {Reason}",
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            domainEvent.Money.Currency,
            domainEvent.Reason);

        var integrationEvent = new TransferFailedIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency,
            domainEvent.Reason
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "TransferFailedIntegrationEvent queued for transaction {TransactionId}, reason {Reason}",
            domainEvent.TransactionId,
            domainEvent.Reason);
    }
}
