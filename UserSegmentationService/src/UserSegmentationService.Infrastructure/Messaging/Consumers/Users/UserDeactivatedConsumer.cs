using System.Text.Json;
using MassTransit;
using UserSegmentationService.Application.IntegrationEvents.Users;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

public class UserDeactivatedConsumer : IConsumer<UserDeactivatedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserDeactivatedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserDeactivatedIntegrationEvent> context)
    {
        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.deactivated",
            JsonSerializer.Serialize(context.Message),
            context.CancellationToken);
    }
}
