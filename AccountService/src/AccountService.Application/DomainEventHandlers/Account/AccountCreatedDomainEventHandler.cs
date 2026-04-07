using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Account;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Account;

public class AccountCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<AccountCreatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<AccountCreatedDomainEventHandler> _logger;

    public AccountCreatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<AccountCreatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AccountCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "AccountCreatedDomainEvent received for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);

        var integrationEvent = new AccountCreatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId,
            domainEvent.AccountNumber.Value,
            domainEvent.Balance.Amount,
            (int)domainEvent.Balance.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "AccountCreatedIntegrationEvent queued for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);
    }
}
