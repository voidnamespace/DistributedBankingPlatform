namespace AuthService.Application.IntegrationEvents;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email
);
