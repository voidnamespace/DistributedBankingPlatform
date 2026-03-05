namespace AccountService.Infrastructure.Messaging.Options;

public sealed class ConsumerOptions
{
    public string Host { get; init; } = "localhost";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";

    public string Exchange { get; init; } = "auth.events";
    public string Queue { get; init; } = "account.events";
}