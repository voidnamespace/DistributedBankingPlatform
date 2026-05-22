using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserSegmentationService.Application.Interfaces.Messaging;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
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
                _logger.LogError(exception, "Failed to process inbox batch");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken) 
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SegmentationDbContext>();

        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var batch = await dbContext.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.OccurredAt)
                    .Take(20)
                    .ToListAsync(cancellationToken);


    }

    public async Task ProcessMessageAsync(
        SegmentationDbContext dbContext,
        )




}
