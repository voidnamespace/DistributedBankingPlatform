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
    private IUnitOfWork _unitOfWork;

    public EvaluateActiveUserSegmentHandler(
        ISegmentCache segmentCache,
        ISegmentMembershipRepository segmentMembershipRepository,
        ISegmentDeltaRepository segmentDeltaRepository,
        IUserMetricRepository userMetricRepository,
        IUnitOfWork unitOfWork)
    {
         _segmentCache = segmentCache;
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentDeltaRepository = segmentDeltaRepository;
        _userMetricRepository = userMetricRepository;
        _unitOfWork = unitOfWork;
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
              _segmentDeltaRepository.Add(
                    SegmentDelta.Create(
                    Guid.NewGuid(),
                    segment.Id,
                    addedUserIds,
                    removedUserIds,
                    DateTime.UtcNow));        
        }

        await _segmentMembershipRepository.ReplaceSegmentMembersAsync(
            segment.Id,
            activeUserIds,
            DateTime.UtcNow,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
