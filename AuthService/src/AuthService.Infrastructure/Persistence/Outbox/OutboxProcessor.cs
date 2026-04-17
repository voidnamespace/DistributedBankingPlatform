using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Messaging.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuthService.Infrastructure.Persistence.Outbox;

public class OutboxProcessor : BackgroundService
{
    private const int MaxPublishAttempts = 5;

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
                await using var transaction =
                    await db.Database.BeginTransactionAsync(stoppingToken);

                var batch = await db.OutboxMessages
                    .FromSqlRaw("""
                        SELECT *
                        FROM "OutboxMessages"
                        WHERE "ProcessedAt" IS NULL
                        ORDER BY "CreatedAt"
                        LIMIT 20
                        FOR UPDATE SKIP LOCKED
                        """)
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
                        var type = IntegrationEventMap.GetType(msg.Type);

                        if (type == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot resolve message type: {msg.Type}");
                        }

                        var integrationEvent = JsonSerializer.Deserialize(
                            msg.Payload,
                            type);

                        if (integrationEvent == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot deserialize message payload: {msg.Id}");
                        }

                        _logger.LogDebug(
                             "Publishing integration event {Type} Id={Id}",
                                 msg.Type,
                                 msg.Id);

                        await publisher.PublishAsync(
                            integrationEvent,
                            msg.Id.ToString("D"),
                            stoppingToken);

                        msg.ProcessedAt = DateTime.UtcNow;
                        msg.Error = null;

                        _logger.LogInformation(
                            "Outbox message published. Id={Id}",
                            msg.Id);
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount += 1;

                        if (msg.AttemptCount >= MaxPublishAttempts)
                        {
                            var deadLetterMessage = new DeadLetterOutboxMessage
                            {
                                Id = Guid.NewGuid(),
                                OriginalOutboxMessageId = msg.Id,
                                Type = msg.Type,
                                Payload = msg.Payload,
                                Error =
                                    $"Max publish attempts reached ({MaxPublishAttempts}). Last error: {ex.Message}",
                                AttemptCount = msg.AttemptCount,
                                CreatedAt = msg.CreatedAt,
                                FinalFailedAt = DateTime.UtcNow
                            };

                            db.DeadLetterOutboxMessages.Add(deadLetterMessage);
                            db.OutboxMessages.Remove(msg);

                            _logger.LogError(
                                ex,
                                "Outbox message moved to dead-letter storage. Id={Id} Attempt={Attempt} DeadLetterId={DeadLetterId}",
                                msg.Id,
                                msg.AttemptCount,
                                deadLetterMessage.Id);
                        }
                        else
                        {
                            msg.Error = ex.Message;

                            _logger.LogError(
                                ex,
                                "Outbox message failed. Id={Id} Attempt={Attempt}",
                                msg.Id,
                                msg.AttemptCount);
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    try
                    {
                        await db.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "OutboxProcessor failed saving processed messages");

                        throw;
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
