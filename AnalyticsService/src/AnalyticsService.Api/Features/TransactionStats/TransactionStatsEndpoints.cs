namespace AnalyticsService.Api.Features.TransactionStats;

public static class TransactionStatsEndpoints
{
    public static IEndpointRouteBuilder MapTransactionStatsFeature(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/analytics/transactions")
            .WithTags("Transaction Analytics");

        group.MapGet("/", () => Results.Ok(TransactionStatsResponse.Empty));

        return app;
    }

    private sealed record TransactionStatsResponse(
        long TotalTransactions,
        decimal TotalAmount,
        decimal AverageAmount,
        DateTime? LastUpdatedAtUtc)
    {
        public static TransactionStatsResponse Empty { get; } = new(
            TotalTransactions: 0,
            TotalAmount: 0,
            AverageAmount: 0,
            LastUpdatedAtUtc: null);
    }
}
