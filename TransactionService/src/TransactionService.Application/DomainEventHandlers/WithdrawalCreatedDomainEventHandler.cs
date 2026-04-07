using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Common;
using TransactionService.Application.IntegrationEvents.Withdrawal;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Domain.Events;

namespace TransactionService.Application.DomainEventHandlers;

public class WithdrawalCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<WithdrawalCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<WithdrawalCreatedDomainEventHandler> _logger;

    public WithdrawalCreatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<WithdrawalCreatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<WithdrawalCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "WithdrawalCreatedDomainEvent received: TransactionId {TransactionId}, InitiatorId {InitiatorId}",
            domainEvent.TransactionId,
            domainEvent.InitiatorId);

        var integrationEvent = new WithdrawalCreatedIntegrationEvent(
            domainEvent.InitiatorId,
            domainEvent.TransactionId,
            domainEvent.FromAccountNumber.Value,
            domainEvent.Money.Amount,
            (int)domainEvent.Money.Currency
        );

        _logger.LogInformation(
            "WithdrawalCreatedIntegrationEvent created: TransactionId {TransactionId}",
            domainEvent.TransactionId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "WithdrawalCreatedIntegrationEvent enqueued to outbox: TransactionId {TransactionId}",
            domainEvent.TransactionId);
    }
}
