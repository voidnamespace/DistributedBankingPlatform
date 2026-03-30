
namespace AccountService.Infrastructure.Messaging.Options;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}
