
namespace TransactionService.Infrastructure.Messaging.Options;

public sealed class AccountEventsConsumerOptions
{
    public string Exchange { get; init; } = "account.transaction.events";

    public string Queue { get; init; } = "transaction.account.events";

}
