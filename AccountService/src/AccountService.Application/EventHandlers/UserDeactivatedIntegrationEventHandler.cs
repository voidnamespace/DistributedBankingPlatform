using AccountService.Application.Commands.ActivateAccount;
using AccountService.Application.Commands.DeactivateAccount;
using AccountService.Application.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.EventHandlers;

public class UserDeactivatedIntegrationEventHandler
    : INotificationHandler<UserDeactivatedIntegrationEvent>
{
    private readonly ILogger<UserDeactivatedIntegrationEventHandler> _logger;

    public UserDeactivatedIntegrationEventHandler(
        ILogger<UserDeactivatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        UserDeactivatedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "UserDeactivatedIntegrationEvent received for user {UserId}",
            notification.UserId);

        var command = new DeactivateAccountCommand(notification.UserId);


    }
}