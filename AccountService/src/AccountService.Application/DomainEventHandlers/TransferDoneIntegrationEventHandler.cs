using MediatR;
using AccountService.Domain.Events;
using AccountService.Application.IntegrationEvents;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Application.Common;

namespace AccountService.Application.DomainEventHandlers;

public class TransferDoneDomainEventHandler
    : INotificationHandler<DomainEventNotification<TransferDoneDomainEvent>>
{
    private readonly IOutboxWriter _outbox;

    public TransferDoneDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
        DomainEventNotification<TransferDoneDomainEvent> notification,
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