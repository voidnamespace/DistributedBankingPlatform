namespace AuthService.Infrastructure.Messaging.RabbitMq.Options;

public sealed class AuthEventsPublisherOptions
{
    public string Exchange { get; init; } = default!;
}
