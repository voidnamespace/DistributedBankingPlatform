using MediatR;
using UserSegmentationService.Application.Commands.Accounts;
using UserSegmentationService.Application.IntegrationEvents.Accounts;

namespace UserSegmentationService.Application.IntegrationEventHandlers.Accounts;

public class AccountCreatedIntegrationEventHandler
    : INotificationHandler<AccountCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public AccountCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        AccountCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CreateUserAccountProjectionCommand(
                notification.UserId,
                notification.AccountId,
                notification.AccountNumber),
            cancellationToken);
    }
}
