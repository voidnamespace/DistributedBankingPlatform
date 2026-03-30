using MediatR;

namespace AccountService.Application.IntegrationEvents.Accounts;

public sealed record AccountActivatedIntegrationEvent(Guid UserId,
    Guid AccountId) : INotification;
