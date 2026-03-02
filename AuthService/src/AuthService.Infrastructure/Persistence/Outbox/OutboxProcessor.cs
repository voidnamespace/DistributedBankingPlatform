using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthService.Infrastructure.Persistence.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
   

    public OutboxProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

                foreach (var msg in batch)
                {
                    try
                    {
                        await publisher.PublishRawAsync(msg.Payload, msg.RoutingKey, stoppingToken);

                        msg.ProcessedAt = DateTime.UtcNow;
                        msg.Error = null;
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
                Console.WriteLine($"[Outbox] ERROR: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}