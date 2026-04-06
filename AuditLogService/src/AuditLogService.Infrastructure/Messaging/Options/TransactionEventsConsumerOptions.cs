namespace AuditLogService.Infrastructure.Messaging.Options;

public sealed record TransactionEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
}
