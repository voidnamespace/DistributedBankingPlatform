using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class AccountActivatedDomainEventHandler : INotificationHandler<DomainEventNotification<AccountActivatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public AccountActivatedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<AccountActivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new AccountActivatedIntegrationEvent(domainEvent.UserId,
            domainEvent.AccountId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
