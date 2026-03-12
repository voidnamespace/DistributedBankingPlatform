using AccountService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AccountService.Application.Interfaces.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace AccountService.Infrastructure.Persistence.Outbox;

public class OutboxProcessor : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    public OutboxProcessor(IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedOnUtc == null)
                    .OrderBy(x => x.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (batch.Count > 0)
                {
                    _logger.LogInformation(
                        "Outbox batch fetched. Count={Count}",
                        batch.Count);
                }


                foreach (var msg in batch)
                {
                    try
                    {
                        await publisher.PublishAsync(
                            msg.Payload,
                            msg.RoutingKey,
                            stoppingToken);

                        msg.OccurredOnUtc = DateTime.UtcNow;
                        msg.Error = null;
                        _logger.LogInformation(
                            "Outbox message published. Id={Id} RoutingKey={RoutingKey}",
                            msg.Id,
                            msg.RoutingKey);
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount += 1;
                        msg.Error = ex.Message;
                        _logger.LogError(
                            ex,
                            "Outbox message failed. Id={Id} Attempt={Attempt}",
                            msg.Id,
                            msg.AttemptCount);
                    }


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxProcessor main loop failure");
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        _logger.LogInformation("OutboxProcessor stopped");
    }
}