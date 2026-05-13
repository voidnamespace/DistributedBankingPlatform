using MediatR;

namespace UserSegmentationService.Application.Commands.Segments.RiskUsers;

public sealed record EvaluateRiskUserSegmentCommand(
    DateTime InactiveSince) : IRequest;
