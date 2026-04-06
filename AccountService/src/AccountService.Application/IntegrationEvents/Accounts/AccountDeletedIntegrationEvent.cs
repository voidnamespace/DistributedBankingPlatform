using MediatR;

namespace AccountService.Application.IntegrationEvents.Accounts;

public sealed record AccountDeletedIntegrationEvent(Guid UserId, Guid AccountId) : INotification;
