using MediatR;
using UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;
using UserSegmentationService.Application.Common.DomainEvents;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Enums;
using UserSegmentationService.Domain.Events;

namespace UserSegmentationService.Application.DomainEventHandlers.SegmentDeltas;

public sealed class SegmentDeltaCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<SegmentDeltaCreatedDomainEvent>>
{
    private readonly ISegmentCache _segmentCache;
    private readonly IMediator _mediator;

    public SegmentDeltaCreatedDomainEventHandler(
        ISegmentCache segmentCache,
        IMediator mediator)
    {
        _segmentCache = segmentCache;
        _mediator = mediator;
    }

    public async Task Handle(
        DomainEventNotification<SegmentDeltaCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var vipSegment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.VipUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        var riskSegment = await _segmentCache.GetByRuleTypeAndKindAsync(
            SegmentRuleType.RiskUsers,
            SegmentKind.Dynamic,
            cancellationToken);

        if (vipSegment == null || riskSegment == null)
            return;

        if (domainEvent.SegmentId != vipSegment.Id &&
            domainEvent.SegmentId != riskSegment.Id)
            return;

        var command = new EvaluateVipAtRiskUserSegmentCommand();

        await _mediator.Send(command, cancellationToken);

    }
}
