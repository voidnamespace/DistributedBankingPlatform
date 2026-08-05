namespace AuthService.Application.IntegrationEvents.Contracts;

public sealed record EmailConfirmedIntegrationEvent(Guid UserId, string Email);
