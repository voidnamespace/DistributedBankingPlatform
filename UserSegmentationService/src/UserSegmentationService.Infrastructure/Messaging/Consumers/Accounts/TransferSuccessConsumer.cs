using System.Text.Json;
using MassTransit;
using UserSegmentationService.Application.IntegrationEvents.Accounts;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Messaging.Consumers.Accounts;

public class TransferSuccessConsumer : IConsumer<TransferSuccessIntegrationEvent>
{
    private readonly IInboxWriter _inboxWriter;

    public TransferSuccessConsumer(IInboxWriter inboxWriter)
    {
        _inboxWriter = inboxWriter;
    }

    public async Task Consume(ConsumeContext<TransferSuccessIntegrationEvent> context)
    {
        if (context.MessageId == null)
            return;

        await _inboxWriter.SaveAsync(
            context.MessageId.Value,
            "transfer.success",
            JsonSerializer.Serialize(context.Message),
            context.CancellationToken);
    }
}
