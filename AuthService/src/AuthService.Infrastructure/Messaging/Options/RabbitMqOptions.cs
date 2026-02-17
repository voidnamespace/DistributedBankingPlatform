namespace AuthService.Infrastructure.Messaging.Options;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "auth.events";
}
