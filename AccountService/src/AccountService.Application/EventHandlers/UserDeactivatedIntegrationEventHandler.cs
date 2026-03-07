using AccountService.Application.IntegrationEvents;
using MediatR;
namespace AccountService.Application.EventHandlers;

public class UserDeactivatedIntegrationEventHandler
: INotificationHandler<UserDeactivatedIntegrationEvent>
{
    public Task Handle(
        UserDeactivatedIntegrationEvent notification,
        CancellationToken ct)
    {
        Console.WriteLine($"User deactivated: {notification.UserId}");

        return Task.CompletedTask;
    }
}
