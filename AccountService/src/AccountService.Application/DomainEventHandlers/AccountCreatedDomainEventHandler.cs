using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events;
using MediatR;

namespace AccountService.Application.DomainEventHandlers;

public class AccountCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<AccountCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;

    public AccountCreatedDomainEventHandler(IOutboxWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(
        DomainEventNotification<AccountCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new AccountCreatedIntegrationEvent(domainEvent.UserId,
            domainEvent.AccountId,
            domainEvent.AccountNumber.Value,
            domainEvent.Balance.Amount,
            (int)domainEvent.Balance.Currency);

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);
    }
}
