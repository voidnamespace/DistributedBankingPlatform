using System.Text.Json;
using MassTransit;
using UserSegmentationService.Application.IntegrationEvents.Users;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

public class UserActivatedConsumer : IConsumer<UserActivatedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserActivatedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserActivatedIntegrationEvent> context)
    {
        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.activated",
            JsonSerializer.Serialize(context.Message),
            context.CancellationToken);
    }
}
