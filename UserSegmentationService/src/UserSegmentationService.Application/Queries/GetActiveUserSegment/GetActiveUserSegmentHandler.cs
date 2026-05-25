using MediatR;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Application.Queries.GetActiveUserSegment;

public class GetActiveUserSegmentHandler
    : IRequestHandler<GetActiveUserSegmentQuery, IReadOnlyList<Guid>>
{
    private readonly IUserMetricRepository _userMetricRepository;

    public GetActiveUserSegmentHandler(IUserMetricRepository userMetricRepository)
    {
        _userMetricRepository = userMetricRepository;
    }

    public Task<IReadOnlyList<Guid>> Handle(
        GetActiveUserSegmentQuery query,
        CancellationToken cancellationToken)
    {
        return _userMetricRepository.GetActiveUserIdsAsync(
            query.ActiveSince,
            cancellationToken);
    }
}
