namespace AuthService.Infrastructure.Messaging.RabbitMq.Options;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public int ChannelPoolMaxSize { get; init; } = 16;
}
