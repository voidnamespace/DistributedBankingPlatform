using MediatR;

namespace UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;

public sealed record  EvaluateVipAtRiskUserSegmentCommand() : IRequest;

