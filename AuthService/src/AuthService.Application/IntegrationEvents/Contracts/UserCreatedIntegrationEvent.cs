namespace AuthService.Application.IntegrationEvents.Contracts;

public sealed record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email);
