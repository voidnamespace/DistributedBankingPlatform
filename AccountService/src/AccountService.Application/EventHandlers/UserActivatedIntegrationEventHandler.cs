using AccountService.Application.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.EventHandlers;

public class UserActivatedIntegrationEventHandler
    : INotificationHandler<UserActivatedIntegrationEvent>
{
    private readonly ILogger<UserActivatedIntegrationEventHandler> _logger;

    public UserActivatedIntegrationEventHandler(
        ILogger<UserActivatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        UserActivatedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "UserActivatedIntegrationEvent received for user {UserId}",
            notification.UserId);

        return Task.CompletedTask;
    }
}