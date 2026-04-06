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
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),

            Type = IntegrationEventMap.GetName(typeof(T)),

            Payload = JsonSerializer.Serialize(integrationEvent),

            CreatedAt = DateTime.UtcNow,

            AttemptCount = 0
        };

        _logger.LogInformation("type of integrationEvent {Type}", integrationEvent.GetType().FullName);

        _context.OutboxMessages.Add(message);

        _logger.LogInformation(
            "Outbox message queued. Type={Type} Id={Id}",
            message.Type,
            message.Id);

        return Task.CompletedTask;
    }
}
