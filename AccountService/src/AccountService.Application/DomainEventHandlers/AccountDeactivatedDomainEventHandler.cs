using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;
namespace AccountService.Application.DomainEventHandlers;

public class AccountDeactivatedDomainEventHandler : INotificationHandler<DomainEventNotification<AccountDeactivatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public AccountDeactivatedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<AccountDeactivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new AccountDeactivatedIntegrationEvent(domainEvent.UserId,
            domainEvent.AccountId);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
