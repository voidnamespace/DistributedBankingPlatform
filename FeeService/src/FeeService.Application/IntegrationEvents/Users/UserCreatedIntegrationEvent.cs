namespace FeeService.Application.IntegrationEvents.Users;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string Currency
);
