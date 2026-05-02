using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Users;

public record UserActivatedIntegrationEvent(Guid UserId) : INotification;
