using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AccountService.Infrastructure.Persistence.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    internal static class JsonDefaults
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

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

                var db = scope.ServiceProvider
                    .GetRequiredService<AccountDbContext>();

                var publisher = scope.ServiceProvider
                    .GetRequiredService<IEventPublisher>();

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
                        var eventType = IntegrationEventTypeMap.GetType(msg.Type);

                        if (eventType == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot resolve message type: {msg.Type}");
                        }

                        _logger.LogInformation(
                            "Resolved integration event CLR type = {Type}",
                            eventType.AssemblyQualifiedName);

                        _logger.LogDebug(
                            "Deserializing integration event payload. Id={Id}",
                            msg.Id);

                        var integrationEvent = JsonSerializer.Deserialize(
                            msg.Payload,
                            eventType,
                            JsonDefaults.Options);

                        if (integrationEvent == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot deserialize message payload: {msg.Id}");
                        }

                        _logger.LogInformation(
                            "Publishing integration event. Id={Id} Type={Type}",
                            msg.Id,
                            msg.Type);

                        await publisher.PublishAsync(
                            integrationEvent,
                            stoppingToken);

                        msg.ProcessedOnUtc = DateTime.UtcNow;
                        msg.Error = null;

                        _logger.LogInformation(
                            "Integration event published successfully. Id={Id} Type={Type}",
                            msg.Id,
                            msg.Type);
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount++;

                        msg.Error = ex.Message;

                        _logger.LogError(
                            ex,
                            "Outbox message publish failed. Id={Id} Attempt={Attempt}",
                            msg.Id,
                            msg.AttemptCount);
                    }
                }

                if (batch.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Outbox batch committed to database. Count={Count}",
                        batch.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OutboxProcessor main loop failure");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }
}
