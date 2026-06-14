namespace AnalyticsService.Api.Infrastructure;

public sealed class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public string ServiceName { get; init; } = "AnalyticsService";
}
