using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Account;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Account;

public class AccountDeactivatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<AccountDeactivatedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<AccountDeactivatedDomainEventHandler> _logger;

    public AccountDeactivatedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<AccountDeactivatedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AccountDeactivatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "AccountDeactivatedDomainEvent received for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);

        var integrationEvent = new AccountDeactivatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "AccountDeactivatedIntegrationEvent queued for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);
    }
}
