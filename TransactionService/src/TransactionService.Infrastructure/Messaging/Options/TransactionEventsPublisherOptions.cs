namespace TransactionService.Infrastructure.Messaging.Options;

public sealed class TransactionEventsPublisherOptions
{
    public string Exchange { get; init; } = default!;
}
