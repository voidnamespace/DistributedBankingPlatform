using MediatR;

namespace UserSegmentationService.Application.Commands.Segments.ActiveUsers;

public sealed record EvaluateActiveUserSegmentCommand(
    DateTime ActiveSince) : IRequest;
