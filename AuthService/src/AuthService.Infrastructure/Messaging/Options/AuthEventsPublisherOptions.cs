namespace AuthService.Infrastructure.Messaging.Options;

public sealed class AuthEventsPublisherOptions
{
    public string Exchange { get; init; } = default!;
}