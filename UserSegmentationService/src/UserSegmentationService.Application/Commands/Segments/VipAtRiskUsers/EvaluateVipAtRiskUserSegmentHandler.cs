using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;

public class EvaluateVipAtRiskUserSegmentHandler
    : IRequestHandler<EvaluateVipAtRiskUserSegmentCommand>
{
    private readonly ISegmentRepository _segmentRepository;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;

    public EvaluateVipAtRiskUserSegmentHandler (
        ISegmentMembershipRepository segmentMembershipRepository, 
        ISegmentRepository segmentRepository)
    {
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentRepository = segmentRepository;
    }

    public async Task Handle(
        EvaluateVipAtRiskUserSegmentCommand command,
        CancellationToken cancellationToken)
    {
        var riskSegment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            SegmentRuleType.RiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (riskSegment == null)
            throw new KeyNotFoundException("Risk users dynamic segment was not found.");
        

        var riskUsers = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(riskSegment.Id, cancellationToken);

        var vipSegment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (vipSegment == null)
            throw new KeyNotFoundException("Risk users dynamic segment was not found.");

        var vipUsers = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(vipSegment.Id, cancellationToken);

        var vipAtRiskUsers = vipUsers
             .Intersect(riskUsers)
             .ToArray();






    }

}
