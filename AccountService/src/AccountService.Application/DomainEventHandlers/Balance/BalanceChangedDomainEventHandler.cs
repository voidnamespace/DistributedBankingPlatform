using AccountService.Application.Common;
using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Events.Balance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.DomainEventHandlers.Balance;

public class BalanceChangedDomainEventHandler
    : INotificationHandler<DomainEventNotification<BalanceChangedDomainEvent>>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<BalanceChangedDomainEventHandler> _logger;

    public BalanceChangedDomainEventHandler(
        IOutboxWriter outboxWriter,
        ILogger<BalanceChangedDomainEventHandler> logger)
    {
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<BalanceChangedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "BalanceChangedDomainEvent received for account {AccountId}, user {UserId}, old balance {OldBalance}, new balance {NewBalance}, currency {Currency}",
            domainEvent.AccountId,
            domainEvent.UserId,
            domainEvent.BalanceOld.Amount,
            domainEvent.BalanceNew.Amount,
            domainEvent.BalanceOld.Currency);

        var integrationEvent = new BalanceChangedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.AccountId,
            domainEvent.BalanceOld.Amount,
            domainEvent.BalanceNew.Amount,
            (int)domainEvent.BalanceOld.Currency
        );

        await _outboxWriter.EnqueueAsync(integrationEvent, ct);

        _logger.LogInformation(
            "BalanceChangedIntegrationEvent queued for account {AccountId}, user {UserId}",
            domainEvent.AccountId,
            domainEvent.UserId);
    }
}
