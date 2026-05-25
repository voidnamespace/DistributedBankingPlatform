using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using MassTransit;
using System.Text.Json;

namespace FeeService.Infrastructure.Messaging.Consuming.UserConsumers;

public class UserDeletedConsumer : IConsumer<UserDeletedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserDeletedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserDeletedIntegrationEvent> context)
    {
        var message = context.Message;

        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.deleted",
            JsonSerializer.Serialize(message),
            context.CancellationToken);
    }

}
