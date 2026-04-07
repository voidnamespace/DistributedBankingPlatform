using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Messaging.Routing;

namespace TransactionService.Infrastructure.Persistence.Outbox;

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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<TransactionDbContext>();

                var publisher = scope.ServiceProvider
                    .GetRequiredService<IEventPublisher>();

                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (batch.Count > 0)
                {
                    _logger.LogInformation(
                        "OutboxProcessor picked batch: Count {Count}",
                        batch.Count);
                }

                foreach (var msg in batch)
                {
                    try
                    {
                        var type = IntegrationEventMap
                            .GetType(msg.Type);

                        if (type == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot resolve message type: {msg.Type}");
                        }

                        var integrationEvent =
                            JsonSerializer.Deserialize(
                                msg.Payload,
                                type);

                        if (integrationEvent == null)
                        {
                            throw new InvalidOperationException(
                                $"Deserialization returned null. MessageId {msg.Id}");
                        }

                        _logger.LogInformation(
                            "Publishing integration event: MessageId {MessageId}, Type {Type}",
                            msg.Id,
                            msg.Type);

                        await publisher.PublishAsync(
                            integrationEvent,
                            stoppingToken);

                        msg.ProcessedAt = DateTime.UtcNow;
                        msg.Error = null;

                        _logger.LogInformation(
                            "Outbox message published successfully: MessageId {MessageId}",
                            msg.Id);
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount += 1;
                        msg.Error = ex.Message;

                        _logger.LogWarning(
                            ex,
                            "Outbox message failed: MessageId {MessageId}, Attempt {AttemptCount}",
                            msg.Id,
                            msg.AttemptCount);
                    }
                }

                if (batch.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "OutboxProcessor batch committed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OutboxProcessor main loop failure");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }
}
