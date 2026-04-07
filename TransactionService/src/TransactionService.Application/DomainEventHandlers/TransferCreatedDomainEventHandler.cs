using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Transfer;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class TransferCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<TransferCreatedDomainEventHandler> _logger;

    public TransferCreatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<TransferCreatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<TransferCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "TransferCreatedDomainEvent received: TransactionId {TransactionId}, InitiatorId {InitiatorId}",
            domainEvent.TransactionId,
            domainEvent.InitiatorId);

        var integrationEvent = new TransferCreatedIntegrationEvent(
            domainEvent.InitiatorId,
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.ToAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        _logger.LogInformation(
            "TransferCreatedIntegrationEvent created: TransactionId {TransactionId}",
            domainEvent.TransactionId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "TransferCreatedIntegrationEvent enqueued to outbox: TransactionId {TransactionId}",
            domainEvent.TransactionId);
    }
}
