using MediatR;

namespace AccountService.Application.IntegrationEvents.Users;

public record UserDeactivatedIntegrationEvent(Guid UserId) : INotification;
