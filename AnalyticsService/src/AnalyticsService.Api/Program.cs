using AnalyticsService.Api.Features.Health;
using AnalyticsService.Api.Features.TransactionStats;
using AnalyticsService.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAnalyticsInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapHealthFeature();
app.MapTransactionStatsFeature();

app.Run();

public partial class Program;
