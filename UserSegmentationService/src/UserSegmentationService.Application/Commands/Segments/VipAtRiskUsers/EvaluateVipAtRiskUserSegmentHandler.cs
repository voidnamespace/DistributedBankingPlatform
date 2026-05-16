using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;

public class EvaluateVipAtRiskUserSegmentHandler
    : IRequestHandler<EvaluateVipAtRiskUserSegmentCommand>
{
    private readonly ISegmentCache _segmentCache;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly ISegmentDeltaRepository _segmentDeltaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EvaluateVipAtRiskUserSegmentHandler (
        ISegmentMembershipRepository segmentMembershipRepository,
        ISegmentCache segmentCache,
        ISegmentDeltaRepository segmentDeltaRepository,
        IUnitOfWork unitOfWork)
    {
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentCache = segmentCache;
        _segmentDeltaRepository = segmentDeltaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        EvaluateVipAtRiskUserSegmentCommand command,
        CancellationToken cancellationToken)
    {
        var riskSegment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.RiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (riskSegment == null)
            throw new KeyNotFoundException("Risk users dynamic segment was not found.");
        

        var riskUsers = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(riskSegment.Id, cancellationToken);

        var vipSegment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (vipSegment == null)
            throw new KeyNotFoundException("Vip users dynamic segment was not found.");

        var vipUsers = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(vipSegment.Id, cancellationToken);

        var newVipAtRiskUsers = vipUsers
             .Intersect(riskUsers)
             .ToArray();

        var vipAtRiskSegment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipAtRiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (vipAtRiskSegment == null)
            throw new KeyNotFoundException("Vip at Risk users dynamic segment was not found.");

        var currentVipAtRiskUserIds = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(
            vipAtRiskSegment.Id,
            cancellationToken);

        var addedUserIds = newVipAtRiskUsers
            .Except(currentVipAtRiskUserIds) 
            .ToArray();

        var removedUserIds = currentVipAtRiskUserIds
            .Except(newVipAtRiskUsers)
            .ToArray();

        var now = DateTime.UtcNow;

        if (addedUserIds.Length > 0 || removedUserIds.Length > 0)
        {
            _segmentDeltaRepository.Add(
                 SegmentDelta.Create(
                 Guid.NewGuid(),
                 vipAtRiskSegment.Id,
                 addedUserIds,
                 removedUserIds,
                 now));

        }

        await _segmentMembershipRepository.ReplaceSegmentMembersAsync(
              vipAtRiskSegment.Id,
              newVipAtRiskUsers,
              now,
              cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }

}
