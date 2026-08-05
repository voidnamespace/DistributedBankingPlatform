using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Common.Events;
using AuthService.Application.IntegrationEvents.Contracts;
using AuthService.Domain.Events;
using MediatR;

namespace AuthService.Application.Features.Users.EmailConfirm;

public class EmailConfirmedDomainEventHandler 
    : INotificationHandler<DomainEventNotification<EmailConfirmedDomainEvent>>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public EmailConfirmedDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher)
    {
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(
        DomainEventNotification<EmailConfirmedDomainEvent> notification, 
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new EmailConfirmedIntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email.Value);

        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
