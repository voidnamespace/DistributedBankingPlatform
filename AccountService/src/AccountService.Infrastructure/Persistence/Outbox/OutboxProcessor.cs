using AccountService.Application.IntegrationEvents.Transactions;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        var map = new Dictionary<string, Type>
        {
            ["transfer.created"] = typeof(TransferCreatedIntegrationEvent),
            ["transfer.failed"] = typeof(TransferFailedIntegrationEvent),
            ["transfer.success"] = typeof(TransferSuccessIntegrationEvent)
        };
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
                        if (!map.TryGetValue(msg.Type, out var eventType))
                        {
                            throw new Exception($"Unknown type: {msg.Type}");
                        }

                        var routingKey = RoutingKeyMap.Get(eventType);

                        await publisher.PublishAsync(
                            msg.Payload,
                            routingKey,
                            stoppingToken);

                        msg.ProcessedOnUtc = DateTime.UtcNow;
                        msg.Error = null;
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount += 1;
                        msg.Error = ex.Message;
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