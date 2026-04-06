using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class BalanceChangedDomainEventHandler : INotificationHandler<DomainEventNotification<BalanceChangedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public BalanceChangedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(DomainEventNotification<BalanceChangedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new BalanceChangedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId,
            domainEvent.BalanceOld.Amount,
            domainEvent.BalanceNew.Amount,
            (int)(domainEvent.BalanceOld.Currency));

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
