using System.Text.Json;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

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
        string routingKey,
        CancellationToken ct)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(integrationEvent),
            CreatedAt = DateTime.UtcNow,
            AttemptCount = 0
        };

        _context.OutboxMessages.Add(message);

        _logger.LogInformation(
            "Outbox message queued. Type={Type} RoutingKey={RoutingKey} Id={Id}",
            message.Type,
            routingKey,
            message.Id);

        return Task.CompletedTask;
    }
}