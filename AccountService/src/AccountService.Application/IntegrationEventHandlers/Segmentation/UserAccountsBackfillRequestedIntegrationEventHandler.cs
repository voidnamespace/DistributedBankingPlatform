using AccountService.Application.Commands.GetUsersAndAccounts;
using AccountService.Application.IntegrationEvents.Segmentation;
using MediatR;

namespace AccountService.Application.IntegrationEventHandlers.Segmentation;

public class UserAccountsBackfillRequestedIntegrationEventHandler : INotificationHandler<UserAccountsBackfillRequestedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserAccountsBackfillRequestedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        UserAccountsBackfillRequestedIntegrationEvent notification,
        CancellationToken ct)
    {
        var command = new GetUsersAndAccountsCommand(
            notification.RequestId, 
            notification.RequestedAt);

        await _mediator.Send(command, ct);
    }

}
