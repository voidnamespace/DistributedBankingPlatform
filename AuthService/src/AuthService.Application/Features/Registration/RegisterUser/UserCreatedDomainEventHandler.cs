using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Application.IntegrationEvents.Contracts;
using AuthService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Registration.RegisterUser;

public class UserCreatedDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UserCreatedDomainEventHandler> logger)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserCreatedDomainEvent> notification,
        CancellationToken ct)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
             "Handling UserCreatedDomainEvent for user {UserId} with email {Email}",
             domainEvent.UserId,
             domainEvent.Email);

        var integrationEvent = new UserCreatedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email.Value);

        _logger.LogInformation(
          "IntegrationEvent type resolved: {IntegrationEventType}",
           integrationEvent.GetType().FullName);

        await _integrationEventPublisher.PublishAsync(
            integrationEvent,
            ct);

        _logger.LogInformation(
         "UserCreatedIntegrationEvent scheduled for publication for user {UserId}",
         domainEvent.UserId);
    }
}
