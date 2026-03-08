using MediatR;

namespace AccountService.Application.IntegrationEvents;

public record UserDeletedIntegrationEvent(Guid UserId) :INotification;

