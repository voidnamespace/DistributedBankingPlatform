namespace AnalyticsService.Api.Features.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthFeature(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/health")
            .WithTags("Health");

        group.MapGet("/", () => Results.Ok(
            new HealthResponse(
                "Healthy",
                "AnalyticsService",
                DateTime.UtcNow)));

        return app;
    }

    private sealed record HealthResponse(
        string Status,
        string Service,
        DateTime CheckedAtUtc);
}
