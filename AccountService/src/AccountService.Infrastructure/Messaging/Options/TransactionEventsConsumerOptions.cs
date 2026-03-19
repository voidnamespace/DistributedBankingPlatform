namespace AccountService.Infrastructure.Messaging.Options;

public class TransactionEventsConsumerOptions
{
    public string Host { get; init; } = "localhost";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";

    public string Exchange { get; init; } = "transaction.service.exchange";
    public string Queue { get; init; } = "account.transaction.events";
}
