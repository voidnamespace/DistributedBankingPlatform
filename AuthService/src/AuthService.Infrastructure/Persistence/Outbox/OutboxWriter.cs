using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuthService.Infrastructure.Persistence.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly AuthDbContext _context;
    private readonly ILogger<OutboxWriter> _logger;

    public OutboxWriter(
        AuthDbContext context,
        ILogger<OutboxWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task EnqueueAsync<T>(
        T integrationEvent,
        CancellationToken ct)
    {
        string payload;

        try
        {
            payload = JsonSerializer.Serialize(integrationEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to serialize integration event {EventType}",
                typeof(T).Name);

            throw;
        }

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),

            Type = IntegrationEventMap.GetName(typeof(T)),

            Payload = payload,

            CreatedAt = DateTime.UtcNow,

            AttemptCount = 0
        };

        _context.OutboxMessages.Add(message);

        _logger.LogInformation(
            "Outbox message queued. Type={Type} Id={Id}",
            message.Type,
            message.Id);

        return Task.CompletedTask;
    }
}
