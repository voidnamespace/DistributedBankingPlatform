using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<EvaluateActiveUserSegmentHandler> _logger;
    private IUnitOfWork _unitOfWork;

    public EvaluateActiveUserSegmentHandler(
        ISegmentCache segmentCache,
        ISegmentMembershipRepository segmentMembershipRepository,
        ISegmentDeltaRepository segmentDeltaRepository,
        IUserMetricRepository userMetricRepository,
        IUnitOfWork unitOfWork,
        ILogger<EvaluateActiveUserSegmentHandler> logger)
    {
         _segmentCache = segmentCache;
        _segmentMembershipRepository = segmentMembershipRepository;
        _segmentDeltaRepository = segmentDeltaRepository;
        _userMetricRepository = userMetricRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        EvaluateActiveUserSegmentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EvaluateActiveUserSegmentCommand started");
        
        var segment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.ActiveUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (segment is null)
        {
            _logger.LogError(
                "Active users segment evaluation failed because dynamic segment was not found. ActiveSince={ActiveSince}",
                command.ActiveSince);

            throw new InvalidOperationException("Active users dynamic segment was not found.");
        }

        var activeUserIds = await _userMetricRepository.GetActiveUserIdsAsync(
            command.ActiveSince,
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

        _logger.LogInformation(
            "Active users segment evaluated. " +
            "SegmentId={SegmentId}, ActiveSince={ActiveSince}, " +
            "ActiveUsersCount={ActiveUsersCount}, PreviousMembersCount={PreviousMembersCount}, AddedUsersCount={AddedUsersCount}," +
            " RemovedUsersCount={RemovedUsersCount}, DeltaCreated={DeltaCreated}",
            segment.Id,
            command.ActiveSince,
            activeUserIds.Count,
            currentUserIds.Count,
            addedUserIds.Length,
            removedUserIds.Length,
            addedUserIds.Length > 0 || removedUserIds.Length > 0);
    }
}
