using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Users;

public record UserDeactivatedIntegrationEvent(Guid UserId) : INotification;
