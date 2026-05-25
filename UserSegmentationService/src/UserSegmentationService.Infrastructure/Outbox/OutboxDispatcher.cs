using System.Text.Json;
using UserSegmentationService.Application.Interfaces.Messaging;

namespace UserSegmentationService.Infrastructure.Outbox;

internal class OutboxDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IEventPublisher _publisher;

    public OutboxDispatcher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var eventType = IntegrationEventTypeMap.GetType(message.Type);

        var integrationEvent = JsonSerializer.Deserialize(
            message.Payload,
            eventType,
            JsonOptions);

        if (integrationEvent is null)
            throw new InvalidOperationException(
                $"Outbox message payload is empty or invalid. MessageId={message.MessageId}, Type={message.Type}");

        await _publisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
