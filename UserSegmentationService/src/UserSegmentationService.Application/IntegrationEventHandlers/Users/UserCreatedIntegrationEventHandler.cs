using MediatR;
using UserSegmentationService.Application.Commands.Users;
using UserSegmentationService.Application.IntegrationEvents.Users;

namespace UserSegmentationService.Application.IntegrationEventHandlers.Users;

public class UserCreatedIntegrationEventHandler
    : INotificationHandler<UserCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        UserCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CreateUserMetricCommand(notification.UserId),
            cancellationToken);
    }
}
