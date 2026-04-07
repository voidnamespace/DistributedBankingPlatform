using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Account;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Account;

public class AccountDeletedDomainEventHandler
    : INotificationHandler<DomainEventNotification<AccountDeletedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<AccountDeletedDomainEventHandler> _logger;

    public AccountDeletedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<AccountDeletedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AccountDeletedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "AccountDeletedDomainEvent received for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);

        var integrationEvent = new AccountDeletedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "AccountDeletedIntegrationEvent queued for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);
    }
}
