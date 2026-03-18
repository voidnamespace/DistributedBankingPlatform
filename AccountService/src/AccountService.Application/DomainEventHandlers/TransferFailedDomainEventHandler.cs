using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;
namespace AccountService.Application.DomainEventHandlers;

public class TransferFailedDomainEventHandler : INotificationHandler<DomainEventNotification<TransferFailedDomainEvent>>
{

    private readonly IOutboxWriter _outbox;


    public TransferFailedDomainEventHandler(IOutboxWriter outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(
      DomainEventNotification<TransferFailedDomainEvent> notification,
      CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new TransferFailedIntegrationEvent(
            domainEvent.FromAccountId,
            domainEvent.ToAccountId,
            domainEvent.Amount,
            domainEvent.Currency
        );

        await _outbox.EnqueueAsync(integrationEvent, "transfer.failed", ct);
    }










}
