using MediatR;
using AccountService.Domain.Events;
using AccountService.Application.IntegrationEvents;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Application.Common;

namespace AccountService.Application.DomainEventHandlers;

public class TransferSuccessDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferSuccessDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public TransferSuccessDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<TransferSuccessDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new TransferCreatedIntegrationEvent(
            domainEvent.FromAccountId,
            domainEvent.ToAccountId,
            domainEvent.Amount,
            domainEvent.Currency
        );

        await _outbox.EnqueueAsync(integrationEvent, "transfer.created", ct);
    }
}