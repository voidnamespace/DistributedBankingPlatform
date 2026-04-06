namespace AccountService.Infrastructure.Messaging.Options;

public sealed class AuthEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
    public string Queue { get; init; } = default!;
}
