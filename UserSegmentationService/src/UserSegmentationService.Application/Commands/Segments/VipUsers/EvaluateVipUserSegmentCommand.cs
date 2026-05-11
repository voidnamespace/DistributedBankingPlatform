using MediatR;

namespace UserSegmentationService.Application.Commands.Segments.VipUsers;

public sealed record EvaluateVipUserSegmentCommand(
    decimal MinimumSpend = 5_000m) : IRequest;
