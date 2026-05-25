using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using MassTransit;
using System.Text.Json;

namespace FeeService.Infrastructure.Messaging.Consuming.UserConsumers;

public class UserActivatedConsumer : IConsumer<UserActivatedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserActivatedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserActivatedIntegrationEvent> context)
    {
        var message = context.Message;

        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.activated",
            JsonSerializer.Serialize(message),
            context.CancellationToken);
    }
}
