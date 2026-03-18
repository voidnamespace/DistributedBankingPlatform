using MediatR;

namespace AccountService.Application.IntegrationEvents.Users;

public record UserActivatedIntegrationEvent(Guid UserId) : INotification;
