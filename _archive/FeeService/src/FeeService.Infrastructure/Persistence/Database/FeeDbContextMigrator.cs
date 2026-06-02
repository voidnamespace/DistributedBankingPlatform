using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeeService.Infrastructure.Persistence.Database;

public sealed class FeeDbContextMigrator : IHostedService
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FeeDbContextMigrator> _logger;

    public FeeDbContextMigrator(
        IServiceScopeFactory scopeFactory,
        ILogger<FeeDbContextMigrator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FeeDbContext>();

                await dbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Database migrations applied successfully");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to apply database migrations on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        using var finalScope = _scopeFactory.CreateScope();
        var finalDbContext = finalScope.ServiceProvider.GetRequiredService<FeeDbContext>();
        await finalDbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
