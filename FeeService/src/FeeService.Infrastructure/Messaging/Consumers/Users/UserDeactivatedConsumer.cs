using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using MassTransit;
using System.Text.Json;

namespace FeeService.Infrastructure.Messaging.Consuming.UserConsumers;

public class UserDeactivatedConsumer : IConsumer<UserDeactivatedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public UserDeactivatedConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserDeactivatedIntegrationEvent> context)
    {
        var message = context.Message;

        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "user.deactivated",
            JsonSerializer.Serialize(message),
            context.CancellationToken);
    }
}
