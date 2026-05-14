using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.VipUsers;

public class EvaluateVipUserSegmentHandler
    : IRequestHandler<EvaluateVipUserSegmentCommand>
{
    private readonly ISegmentCache _segmentCache;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly ISegmentDeltaRepository _segmentDeltaRepository;
    private readonly IUserMetricRepository _userMetricRepository;

    public EvaluateVipUserSegmentHandler(
        ISegmentCache segmentCache,
        ISegmentMembershipRepository segmentMembershipRepository,
        ISegmentDeltaRepository segmentDeltaRepository,
        IUserMetricRepository userMetricRepository)
    {
        _segmentCache = segmentCache;
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentDeltaRepository = segmentDeltaRepository;
        _userMetricRepository = userMetricRepository;
    }

    public async Task Handle(
        EvaluateVipUserSegmentCommand request,
        CancellationToken cancellationToken)
    {
        var segment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (segment is null)
            throw new InvalidOperationException("VIP users dynamic segment was not found.");

        var vipUserIds = await _userMetricRepository.GetVipUserIdsAsync(
            request.MinimumSpend,
            cancellationToken);

        var currentUserIds = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(
            segment.Id,
            cancellationToken);

        var addedUserIds = vipUserIds
            .Except(currentUserIds)
            .ToArray();

        var removedUserIds = currentUserIds
            .Except(vipUserIds)
            .ToArray();

        if (addedUserIds.Length > 0 || removedUserIds.Length > 0)
        {
            await _segmentDeltaRepository.AddAsync(
                new SegmentDelta(
                    Guid.NewGuid(),
                    segment.Id,
                    addedUserIds,
                    removedUserIds,
                    DateTime.UtcNow),
                cancellationToken);
        }

        await _segmentMembershipRepository.ReplaceSegmentMembersAsync(
            segment.Id,
            vipUserIds,
            DateTime.UtcNow,
            cancellationToken);
    }
}
