using System.Text.Json;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Persistence.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly AuthDbContext _context;

    public OutboxWriter(AuthDbContext context)
    {
        _context = context;
    }

    public Task EnqueueAsync<T>(T integrationEvent, string routingKey, CancellationToken ct)
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
        return Task.CompletedTask;
    }
}