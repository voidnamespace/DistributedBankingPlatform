using AccountService.Application.Commands.DeleteAccountEventChain;
using AccountService.Application.IntegrationEvents.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.IntegrationEventHandlers.Users;

public class UserDeletedIntegrationEventHandler
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserDeletedIntegrationEventHandler> _logger;

    public UserDeletedIntegrationEventHandler(
        IMediator mediator,
        ILogger<UserDeletedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        UserDeletedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "UserDeletedIntegrationEvent received for user {UserId}",
            notification.UserId);

        var command = new DeleteAccountEventChainCommand(notification.UserId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "DeleteAccountEventChainCommand sent for user {UserId}",
            notification.UserId);
    }
}