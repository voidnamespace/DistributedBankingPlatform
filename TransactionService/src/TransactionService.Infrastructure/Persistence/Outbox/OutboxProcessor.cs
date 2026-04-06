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

    public OutboxProcessor(IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory; 
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

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
                            throw new Exception("Deserialization returned null");

                        _logger.LogInformation("integration event type = {Type}", integrationEvent.GetType().FullName);

                        await publisher.PublishAsync(
                            integrationEvent, 
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
                        msg.Error = ex.Message;

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
    }
}
