using MediatR;

namespace AccountService.Application.IntegrationEvents.Users;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string Currency
) : INotification;