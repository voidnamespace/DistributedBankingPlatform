using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Segments.ActiveUsers;

public class EvaluateActiveUserSegmentCommandHandler
    : IRequestHandler<EvaluateActiveUserSegmentCommand>
{
    private readonly ISegmentRepository _segmentRepository;
    private readonly ISegmentMembershipRepository _segmentMembershipRepository;
    private readonly IUserMetricRepository _userMetricRepository;

    public EvaluateActiveUserSegmentCommandHandler(
        ISegmentRepository segmentRepository,
        ISegmentMembershipRepository segmentMembershipRepository,
        IUserMetricRepository userMetricRepository)
    {
        _segmentRepository = segmentRepository;
        _segmentMembershipRepository = segmentMembershipRepository;
        _userMetricRepository = userMetricRepository;
    }

    public async Task Handle(
        EvaluateActiveUserSegmentCommand request,
        CancellationToken cancellationToken)
    {
        var segment = await _segmentRepository.GetByRuleTypeAndKindAsync(
            SegmentRuleType.ActiveUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (segment is null)
            throw new InvalidOperationException("Active users dynamic segment was not found.");

        var activeUserIds = await _userMetricRepository.GetActiveUserIdsAsync(
            request.ActiveSince,
            cancellationToken);

        await _segmentMembershipRepository.ReplaceSegmentMembersAsync(
            segment.Id,
            activeUserIds,
            DateTime.UtcNow,
            cancellationToken);
    }
}
