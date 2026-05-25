using MediatR;
using UserSegmentationService.Application.Commands.Accounts.CreateProjection;
using UserSegmentationService.Application.IntegrationEvents.Accounts;

namespace UserSegmentationService.Application.IntegrationEventHandlers.Accounts;

public class UserAccountsBackfillBatchProvidedIntegrationEventHandler : INotificationHandler<UserAccountsBackfillBatchProvidedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserAccountsBackfillBatchProvidedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        UserAccountsBackfillBatchProvidedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        foreach (var account in notification.Accounts)
        {
            await _mediator.Send(
                new CreateUserAccountProjectionCommand(
                    account.UserId,
                    account.AccountId,
                    account.AccountNumber),
                cancellationToken);
        }
    }
}
