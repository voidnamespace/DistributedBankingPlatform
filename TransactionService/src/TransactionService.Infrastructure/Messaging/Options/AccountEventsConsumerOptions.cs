
namespace TransactionService.Infrastructure.Messaging.Options;

public sealed class AccountEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;

    public string Queue { get; init; } = default!;

}
