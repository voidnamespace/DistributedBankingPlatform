using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Messaging.Routing;

namespace TransactionService.Infrastructure.Persistence.Inbox;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InboxProcessor> _logger;

    internal static class JsonDefaults
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public InboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<InboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("InboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<TransactionDbContext>();

                var mediator = scope.ServiceProvider
                    .GetRequiredService<IMediator>();

                var batch = await db.InboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.ReceivedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (batch.Count > 0)
                {
                    _logger.LogInformation(
                        "InboxProcessor picked batch: Count {Count}",
                        batch.Count);
                }

                foreach (var message in batch)
                {
                    try
                    {
                        var type = IntegrationEventMap
                            .GetType(message.Type);

                        if (type is null)
                        {
                            message.AttemptCount++;
                            message.Error = "Type not found";

                            _logger.LogWarning(
                                "Inbox message type resolution failed: MessageId {MessageId}, Type {Type}",
                                message.Id,
                                message.Type);

                            continue;
                        }

                        var integrationEvent =
                            JsonSerializer.Deserialize(
                                message.Payload,
                                type,
                                JsonDefaults.Options);

                        if (integrationEvent is not INotification notification)
                        {
                            throw new InvalidOperationException(
                                $"Message is not MediatR notification. MessageId {message.Id}");
                        }

                        _logger.LogInformation(
                            "Publishing integration event to mediator: MessageId {MessageId}, Type {Type}",
                            message.Id,
                            message.Type);

                        await mediator.Publish(
                            notification,
                            stoppingToken);

                        message.ProcessedAt = DateTime.UtcNow;
                        message.Error = null;

                        _logger.LogInformation(
                            "Inbox message processed successfully: MessageId {MessageId}",
                            message.Id);
                    }
                    catch (Exception ex)
                    {
                        message.AttemptCount++;
                        message.Error = ex.Message;

                        _logger.LogWarning(
                            ex,
                            "Inbox message failed: MessageId {MessageId}, Attempt {AttemptCount}",
                            message.Id,
                            message.AttemptCount);
                    }
                }

                if (batch.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "InboxProcessor batch committed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "InboxProcessor main loop failure");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }

        _logger.LogInformation("InboxProcessor stopped");
    }
}
