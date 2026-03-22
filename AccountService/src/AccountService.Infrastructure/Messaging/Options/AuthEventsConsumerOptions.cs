namespace AccountService.Infrastructure.Messaging.Options;

public sealed class AuthEventsConsumerOptions
{
    public string Exchange { get; init; } = "auth.events";
    public string Queue { get; init; } = "account.auth.events";
}
