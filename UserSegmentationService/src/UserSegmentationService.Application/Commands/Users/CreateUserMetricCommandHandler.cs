using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Users;

public class CreateUserMetricCommandHandler
    : IRequestHandler<CreateUserMetricCommand>
{
    private readonly IUserMetricRepository _userMetricRepository;

    public CreateUserMetricCommandHandler(
        IUserMetricRepository userMetricRepository)
    {
        _userMetricRepository = userMetricRepository;
    }

    public async Task Handle(
        CreateUserMetricCommand request,
        CancellationToken cancellationToken)
    {
        var alreadyExists = await _userMetricRepository.ExistsAsync(
            request.UserId,
            cancellationToken);

        if (alreadyExists)
            return;

        _userMetricRepository.Add(new UserMetric(request.UserId));
    }
}
