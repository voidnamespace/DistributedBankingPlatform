using AccountService.Application.Commands.DeactivateAccountEventChain;
using AccountService.Application.IntegrationEvents.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.IntegrationEventHandlers.Users;

public class UserDeactivatedIntegrationEventHandler
    : INotificationHandler<UserDeactivatedIntegrationEvent>
{
    private readonly ILogger<UserDeactivatedIntegrationEventHandler> _logger;
    private readonly IMediator _mediator;

    public UserDeactivatedIntegrationEventHandler(
        ILogger<UserDeactivatedIntegrationEventHandler> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(
        UserDeactivatedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "UserDeactivatedIntegrationEvent received for user {UserId}",
            notification.UserId);

        var command = new DeactivateAccountsForDeactivatedUserCommand(notification.UserId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "DeleteAccountEventChainCommand sent for user {UserId}",
            notification.UserId);
    }
}
