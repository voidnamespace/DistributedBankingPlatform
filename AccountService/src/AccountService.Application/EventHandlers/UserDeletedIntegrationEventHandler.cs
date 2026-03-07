using AccountService.Application.Commands.DeleteAccountEventChain;
using AccountService.Application.IntegrationEvents;
using MediatR;

namespace AccountService.Application.EventHandlers;

public class UserDeletedIntegrationEventHandler
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserDeletedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        UserDeletedIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = new DeleteAccountEventChainCommand(notification.UserId);

        await _mediator.Send(command, ct);
    }
}
