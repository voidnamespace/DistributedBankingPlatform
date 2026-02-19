namespace AccountService.Application.IntegrationEvents;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string Currency
);
