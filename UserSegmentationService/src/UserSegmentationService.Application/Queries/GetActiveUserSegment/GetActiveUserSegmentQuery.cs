using MediatR;

namespace UserSegmentationService.Application.Queries.GetActiveUserSegment;

public sealed record GetActiveUserSegmentQuery(
    DateTime ActiveSince) : IRequest<IReadOnlyList<Guid>>;
