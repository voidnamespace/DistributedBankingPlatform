using AccountService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AccountService.Infrastructure.Persistence.Inbox;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InboxProcessor> _logger;

    public InboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<InboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InboxProcessor started");

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

            if (messages.Count > 0)
            {
                _logger.LogInformation(
                    "Inbox batch fetched. Count={Count}",
                    messages.Count);
            }

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

                        _logger.LogWarning(
                            "Inbox message type not found. Id={Id} Type={Type}",
                            message.Id,
                            message.Type);

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

                    _logger.LogInformation(
                        "Inbox message processed. Id={Id} Type={Type}",
                        message.Id,
                        message.Type);
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.Error = ex.Message;

                    _logger.LogError(
                        ex,
                        "Inbox message failed. Id={Id} Attempt={Attempt}",
                        message.Id,
                        message.AttemptCount);
                }
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}