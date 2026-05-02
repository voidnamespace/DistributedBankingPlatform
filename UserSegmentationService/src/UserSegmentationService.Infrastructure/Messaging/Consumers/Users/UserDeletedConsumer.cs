using System.Text.Json;
using MassTransit;
using UserSegmentationService.Application.IntegrationEvents.Users;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

public class UserDeletedConsumer : IConsumer<UserDeletedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserDeletedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserDeletedIntegrationEvent> context)
    {
        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.deleted",
            JsonSerializer.Serialize(context.Message),
            context.CancellationToken);
    }
}
