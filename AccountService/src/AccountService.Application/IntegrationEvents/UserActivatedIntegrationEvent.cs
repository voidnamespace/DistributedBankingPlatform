using MediatR;

namespace AccountService.Application.IntegrationEvents;

public record UserActivatedIntegrationEvent(Guid UserId) : INotification;
