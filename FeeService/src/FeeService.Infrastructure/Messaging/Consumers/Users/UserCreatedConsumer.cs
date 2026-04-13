using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using MassTransit;
using System.Text.Json;

namespace FeeService.Infrastructure.Messaging.Consuming.UserConsumers;

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
