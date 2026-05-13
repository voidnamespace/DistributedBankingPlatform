using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.RiskUsers;

public class EvaluateRiskUserSegmentHandler
    : IRequestHandler<EvaluateRiskUserSegmentCommand>
{
    private readonly ISegmentRepository _segmentRepository;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly ISegmentDeltaRepository _segmentDeltaRepository;
    private readonly IUserMetricRepository _userMetricRepository;

    public EvaluateRiskUserSegmentHandler(
        ISegmentRepository segmentRepository,
        ISegmentMembershipRepository segmentMembershipRepository,
        ISegmentDeltaRepository segmentDeltaRepository,
        IUserMetricRepository userMetricRepository)
    {
        _segmentRepository = segmentRepository;
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentDeltaRepository = segmentDeltaRepository;
        _userMetricRepository = userMetricRepository;
    }

    public async Task Handle(
        EvaluateRiskUserSegmentCommand request,
        CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            SegmentRuleType.RiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (segment is null)
            throw new InvalidOperationException("Risk users dynamic segment was not found.");

        var riskUserIds = await _userMetricRepository.GetRiskUserIdsAsync(
            request.InactiveSince,
            cancellationToken);

        var currentUserIds = await _segmentMembershipRepository.GetUserIdsBySegmentIdAsync(
            segment.Id,
            cancellationToken);

        var addedUserIds = riskUserIds
            .Except(currentUserIds)
            .ToArray();

        var removedUserIds = currentUserIds
            .Except(riskUserIds)
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
            riskUserIds,
            DateTime.UtcNow,
            cancellationToken);
    }
}
