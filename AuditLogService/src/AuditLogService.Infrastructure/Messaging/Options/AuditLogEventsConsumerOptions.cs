
namespace AuditLogService.Infrastructure.Messaging.Options;

public sealed class AuditLogEventsConsumerOptions
{
    public string Queue { get; init; } = "auditlog.events";
}
