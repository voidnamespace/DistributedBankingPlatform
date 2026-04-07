using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Account;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Account;

public class AccountActivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<AccountActivatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<AccountActivatedDomainEventHandler> _logger;

    public AccountActivatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<AccountActivatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AccountActivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "AccountActivatedDomainEvent received for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);

        var integrationEvent = new AccountActivatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "AccountActivatedIntegrationEvent queued for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);
    }
}
