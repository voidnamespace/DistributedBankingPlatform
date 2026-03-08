using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuthService.Infrastructure.Persistence.Outbox;

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
        _logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
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
                        var envelope = new
                        {
                            Type = msg.Type,
                            Payload = JsonSerializer.Deserialize<object>(msg.Payload)
                        };

                        var json = JsonSerializer.Serialize(envelope);

                        await publisher.PublishRawAsync(
                            json,
                            msg.RoutingKey,
                            stoppingToken);

                        msg.ProcessedAt = DateTime.UtcNow;
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

                if (batch.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
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