using MediatR;

namespace AccountService.Application.IntegrationEvents;

public record UserDeactivatedIntegrationEvent(Guid UserId) : INotification;
