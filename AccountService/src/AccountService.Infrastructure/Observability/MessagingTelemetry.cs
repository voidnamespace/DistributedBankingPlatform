using System.Diagnostics;

namespace AccountService.Infrastructure.Observability;

public static class MessagingTelemetry
{
    public const string ActivitySourceName = "AccountService.Messaging";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}