using MediatR;

namespace AccountService.Application.IntegrationEvents.Accounts;

public sealed record AccountDeactivatedIntegrationEvent(Guid UserId,
    Guid AccountId) : INotification;
