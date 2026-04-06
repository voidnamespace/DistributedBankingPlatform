using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class AccountDeletedDomainEventHandler : INotificationHandler<DomainEventNotification<AccountDeletedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public AccountDeletedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(DomainEventNotification<AccountDeletedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new AccountDeletedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }

}
