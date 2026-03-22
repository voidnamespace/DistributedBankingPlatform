
namespace TransactionService.Infrastructure.Messaging.Options;

public sealed class AccountEventsConsumerOptions
{
    public string Host { get; init; } = "localhost";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "account.events";

    public string Queue { get; init; } = "asd";

    HHAT IS THIS DUDE WHY U HAVE HERE RABBIT MQ OPTITONS WHAT TO DOE ^!!???
}
