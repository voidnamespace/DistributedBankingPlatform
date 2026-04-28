using MassTransit;
using System.Text.Json;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Application.IntegrationEvents.Users;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserCreatedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.created",
            JsonSerializer.Serialize(message),
            context.CancellationToken);
    }
}