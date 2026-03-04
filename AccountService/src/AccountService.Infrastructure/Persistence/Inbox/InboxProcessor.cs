using AccountService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace AccountService.Infrastructure.Persistence.Inbox;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public InboxProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AccountDbContext>();

            var mediator = scope.ServiceProvider
                .GetRequiredService<IMediator>();

            var messages = await db.InboxMessages
                .Where(x => x.ProcessedAt == null)
                .OrderBy(x => x.ReceivedAt)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    var type = Type.GetType(
    $"AccountService.Application.IntegrationEvents.{message.Type}, AccountService.Application");

                    if (type is null)
                    {
                        message.Error = "Type not found";
                        message.AttemptCount++;
                        continue;
                    }

                    var integrationEvent = JsonSerializer.Deserialize(
                        message.Payload,
                        type);

                    if (integrationEvent is INotification notification)
                    {
                        await mediator.Publish(notification, stoppingToken);
                    }

                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.Error = ex.Message;
                }
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}