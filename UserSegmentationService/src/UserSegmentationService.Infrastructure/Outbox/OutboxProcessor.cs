using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Outbox;

internal class OutboxProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process outbox batch");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SegmentationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
        var now = DateTime.UtcNow;

        var batch = await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .Where(x => x.NextAttemptAt == null || x.NextAttemptAt <= now)
            .OrderBy(x => x.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            await ProcessMessageAsync(
                dbContext,
                dispatcher,
                message,
                cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(
        SegmentationDbContext dbContext,
        OutboxDispatcher dispatcher,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.DispatchAsync(message, cancellationToken);

            message.MarkProcessed(DateTime.UtcNow);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish outbox message {MessageId} of type {MessageType}",
                message.MessageId,
                message.Type);

            message.MarkFailed(exception.Message, DateTime.UtcNow);

            if (message.AttemptCount >= MaxAttempts)
            {
                dbContext.DeadLetterOutboxMessages.Add(
                    DeadLetterOutboxMessage.From(
                        message,
                        exception.ToString(),
                        DateTime.UtcNow));

                dbContext.OutboxMessages.Remove(message);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
