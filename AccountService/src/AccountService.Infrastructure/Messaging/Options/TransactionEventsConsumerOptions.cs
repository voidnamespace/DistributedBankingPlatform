namespace AccountService.Infrastructure.Messaging.Options;

public class TransactionEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
    public string Queue { get; init; } = default!;
}
