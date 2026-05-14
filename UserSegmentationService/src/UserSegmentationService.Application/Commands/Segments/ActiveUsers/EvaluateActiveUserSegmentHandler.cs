using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.ActiveUsers;

public class EvaluateActiveUserSegmentHandler
    : IRequestHandler<EvaluateActiveUserSegmentCommand>
{
    private readonly ISegmentCache _segmentCache;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly ISegmentDeltaRepository _segmentDeltaRepository;
    private readonly IUserMetricRepository _userMetricRepository;

    public EvaluateActiveUserSegmentHandler(
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
        EvaluateActiveUserSegmentCommand request,
        CancellationToken cancellationToken)
    {
        var segment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.ActiveUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (segment is null)
            throw new InvalidOperationException("Active users dynamic segment was not found.");

        var activeUserIds = await _userMetricRepository.GetActiveUserIdsAsync(
            request.ActiveSince,
            cancellationToken);

        var currentUserIds = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(
            segment.Id,
            cancellationToken);

        var addedUserIds = activeUserIds
            .Except(currentUserIds)
            .ToArray();

        var removedUserIds = currentUserIds
            .Except(activeUserIds)
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
            activeUserIds,
            DateTime.UtcNow,
            cancellationToken);
    }
}
