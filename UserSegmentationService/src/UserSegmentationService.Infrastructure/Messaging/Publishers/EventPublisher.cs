using MassTransit;
using UserSegmentationService.Application.Interfaces.Messaging;

namespace UserSegmentationService.Infrastructure.Messaging.Publishers;

internal class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(
        T message,
        CancellationToken ct)
    {
        return _publishEndpoint.Publish(message, ct);
    }
}
