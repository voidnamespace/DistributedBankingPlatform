
namespace AuditLogService.Infrastructure.Messaging.Options;

public sealed class AuthEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
}
