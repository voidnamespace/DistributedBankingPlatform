using System.Diagnostics;

namespace TransactionService.Infrastructure.Observability;

public static class MessagingTelemetry
{
    public const string ActivitySourceName = "TransactionService.Messaging";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}
