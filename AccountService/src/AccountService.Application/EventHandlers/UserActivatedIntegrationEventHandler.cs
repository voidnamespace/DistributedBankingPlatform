using AccountService.Application.IntegrationEvents;
using MediatR;
namespace AccountService.Application.EventHandlers;

public class UserActivatedIntegrationEventHandler
    : INotificationHandler<UserActivatedIntegrationEvent>
{
    public Task Handle(
        UserActivatedIntegrationEvent notification,
        CancellationToken ct)
    {
        Console.WriteLine($"User activated: {notification.UserId}");

        return Task.CompletedTask;
    }
}