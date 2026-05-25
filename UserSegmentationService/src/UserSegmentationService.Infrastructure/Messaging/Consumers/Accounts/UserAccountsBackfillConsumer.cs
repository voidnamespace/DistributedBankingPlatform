using MassTransit;
using System.Text.Json;
using UserSegmentationService.Application.IntegrationEvents.Accounts;
using UserSegmentationService.Application.Interfaces.Messaging;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Accounts;

public class UserAccountsBackfillConsumer : IConsumer<UserAccountsBackfillBatchProvidedIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public  UserAccountsBackfillConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<UserAccountsBackfillBatchProvidedIntegrationEvent> context)
    {
        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "account.backfill",
            JsonSerializer.Serialize(context.Message),
            context.CancellationToken);
    }
}
