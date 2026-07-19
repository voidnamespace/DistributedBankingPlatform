namespace AuthService.Application.IntegrationEvents.Contracts;

public sealed record UserDeactivatedIntegrationEvent(Guid UserId);
