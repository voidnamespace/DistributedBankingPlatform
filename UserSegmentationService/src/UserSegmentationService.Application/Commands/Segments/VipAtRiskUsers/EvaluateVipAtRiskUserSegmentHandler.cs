using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;

public class EvaluateVipAtRiskUserSegmentHandler
    : IRequestHandler<EvaluateVipAtRiskUserSegmentCommand>
{
    private readonly ISegmentRepository _segmentRepository;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly ISegmentDeltaRepository _segmentDeltaRepository;

    public EvaluateVipAtRiskUserSegmentHandler (
        ISegmentMembershipRepository segmentMembershipRepository, 
        ISegmentRepository segmentRepository,
        ISegmentDeltaRepository segmentDeltaRepository)
    {
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentRepository = segmentRepository;
        _segmentDeltaRepository = segmentDeltaRepository;
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
            throw new KeyNotFoundException("Vip users dynamic segment was not found.");

        var vipUsers = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(vipSegment.Id, cancellationToken);

        var vipAtRiskUsers = vipUsers
             .Intersect(riskUsers)
             .ToArray();

        var vipAtRiskSegment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipAtRiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (vipAtRiskSegment == null)
            throw new KeyNotFoundException("Vip at Risk users dynamic segment was not found.");

        var currenctVipAtRiskUserIds = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(
            vipAtRiskSegment.Id,
            cancellationToken);

        var addedUserIds = vipAtRiskUsers
            .Except(currenctVipAtRiskUserIds) 
            .ToArray();

        var removedUserIds = currenctVipAtRiskUserIds
            .Except(vipAtRiskUsers)
            .ToArray();

        if (addedUserIds.Length > 0 || removedUserIds.Length > 0)
        {
            await _segmentDeltaRepository.AddAsync(
                new SegmentDelta(
                    Guid.NewGuid(),
                    vipAtRiskSegment.Id,
                    addedUserIds,
                    removedUserIds,
                    DateTime.UtcNow),
                cancellationToken);
        }

        await _segmentMembershipRepository.ReplaceSegmentMembersAsync(
              vipAtRiskSegment.Id,
              vipAtRiskUsers,
              DateTime.UtcNow,
              cancellationToken);

    }

}
