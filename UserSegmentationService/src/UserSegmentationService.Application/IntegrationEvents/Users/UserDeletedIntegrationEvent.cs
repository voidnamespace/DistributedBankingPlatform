using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Users;

public record UserDeletedIntegrationEvent(Guid UserId) : INotification;
