namespace AuthService.Application.IntegrationEvents.Contracts;

public sealed record UserDeletedIntegrationEvent(Guid UserId);
