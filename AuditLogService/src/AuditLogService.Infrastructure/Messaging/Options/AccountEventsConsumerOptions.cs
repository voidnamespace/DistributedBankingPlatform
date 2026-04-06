namespace AuditLogService.Infrastructure.Messaging.Options;

public sealed class AccountEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
}
