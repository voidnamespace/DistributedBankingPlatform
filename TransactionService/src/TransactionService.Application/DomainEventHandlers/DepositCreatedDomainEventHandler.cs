using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Deposit;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class DepositCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<DepositCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<DepositCreatedDomainEventHandler> _logger;

    public DepositCreatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<DepositCreatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<DepositCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "DepositCreatedDomainEvent received: TransactionId {TransactionId}",
            domainEvent.TransactionId);

        var integrationEvent = new DepositCreatedIntegrationEvent(
            domainEvent.TransactionId,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency);

        _logger.LogInformation(
            "DepositCreatedIntegrationEvent created: TransactionId {TransactionId}",
            domainEvent.TransactionId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "DepositCreatedIntegrationEvent enqueued to outbox: TransactionId {TransactionId}",
            domainEvent.TransactionId);
    }
}
