using MediatR;

namespace UserSegmentationService.Application.Commands.Users;

public record CreateUserMetricCommand(Guid UserId) : IRequest;
