namespace AccountService.Infrastructure.Messaging.Options;

public class TransactionEventsConsumerOptions
{
    public string Exchange { get; init; } = "transaction.service.exchange";
    public string Queue { get; init; } = "account.transaction.events";
}
