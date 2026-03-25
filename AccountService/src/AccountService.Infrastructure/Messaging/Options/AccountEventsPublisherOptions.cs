namespace AccountService.Infrastructure.Messaging.Options;

public sealed class AccountEventsPublisherOptions
{
    public string Exchange { get; init; } = "account.transaction.events";
}
