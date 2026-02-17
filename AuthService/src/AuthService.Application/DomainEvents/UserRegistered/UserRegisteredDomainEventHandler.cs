using MediatR;
using Microsoft.Extensions.Logging;
using AuthService.Domain.Events;

public sealed class UserRegisteredDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;

    public UserRegisteredDomainEventHandler(
        ILogger<UserRegisteredDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<UserRegisteredEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "User registered domain event fired. UserId: {UserId}",
            domainEvent.UserId);

        return Task.CompletedTask;
    }
}
