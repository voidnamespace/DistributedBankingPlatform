using MediatR;

namespace AccountService.Application.IntegrationEvents.Users;

public record UserDeletedIntegrationEvent(Guid UserId) :INotification;

