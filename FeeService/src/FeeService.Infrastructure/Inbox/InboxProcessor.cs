using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Persistence.Database;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeeService.Infrastructure.Inbox;

public sealed class InboxProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

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
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing inbox messages");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<FeeDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInboxDispatcher>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        var messages = await dbContext.InboxMessages
            .FromSqlInterpolated($"""
                SELECT "Id", "Type", "Payload", "Processed", "ReceivedAt", "ProcessedAt", "AttemptCount", "Error"
                FROM "InboxMessages"
                WHERE NOT "Processed" AND "AttemptCount" < {MaxAttempts}
                ORDER BY "ReceivedAt"
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(
                    message.Id,
                    message.Type,
                    message.Payload,
                    cancellationToken);

                message.Processed = true;
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.Error = ex.Message;

                if (message.AttemptCount >= MaxAttempts)
                {
                    await DeadLetterMessageAsync(dbContext, message, cancellationToken);
                    message.Processed = true;
                    message.ProcessedAt = DateTime.UtcNow;

                    _logger.LogError(
                        ex,
                        "Inbox message {MessageId} moved to dead-letter storage after {AttemptCount} attempts",
                        message.Id,
                        message.AttemptCount);
                }

                _logger.LogWarning(
                    ex,
                    "Inbox message {MessageId} failed on attempt {Attempt}/{MaxAttempts}",
                    message.Id,
                    message.AttemptCount,
                    MaxAttempts);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task DeadLetterMessageAsync(
        FeeDbContext dbContext,
        InboxMessage message,
        CancellationToken cancellationToken)
    {
        var alreadyDeadLettered = await dbContext.DeadLetterInboxMessages
            .AnyAsync(x => x.MessageId == message.Id, cancellationToken);

        if (alreadyDeadLettered)
            return;

        var deadLetter = new DeadLetterInboxMessage
        {
            MessageId = message.Id,
            Type = message.Type,
            Payload = message.Payload,
            ReceivedAt = message.ReceivedAt,
            AttemptCount = message.AttemptCount,
            Error = message.Error,
            DeadLetteredAt = DateTime.UtcNow
        };

        await dbContext.DeadLetterInboxMessages.AddAsync(deadLetter, cancellationToken);
    }
}
