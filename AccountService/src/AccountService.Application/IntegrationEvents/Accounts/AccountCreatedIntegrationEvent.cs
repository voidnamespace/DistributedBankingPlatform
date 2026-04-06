using MediatR;

namespace AccountService.Application.IntegrationEvents.Accounts;

public sealed record AccountCreatedIntegrationEvent(
    Guid UserId,
    Guid AccountId,
    string AccountNumber,
    decimal Amount,
    int Currency) : INotification;
