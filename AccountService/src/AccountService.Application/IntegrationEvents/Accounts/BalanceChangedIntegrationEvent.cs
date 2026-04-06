using MediatR;

namespace AccountService.Application.IntegrationEvents.Accounts;

public sealed record BalanceChangedIntegrationEvent(
    Guid UserId,
    Guid AccountId,
    decimal OldBalance,
    decimal NewBalance,
    int Currency) : INotification;
