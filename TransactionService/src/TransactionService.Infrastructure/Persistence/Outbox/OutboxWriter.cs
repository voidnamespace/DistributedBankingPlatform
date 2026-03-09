using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Data;
namespace TransactionService.Infrastructure.Persistence.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly TransactionDbContext _context;
    private readonly ILogger<OutboxWriter> _logger;


    public OutboxWriter(TransactionDbContext context, ILogger<OutboxWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task EnqueueAsync<T>(T integrationEvent, string routingKey, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(integrationEvent),
            CreatedAt = DateTime.UtcNow,
            AttemptCount = 0,
        };

        _context.OutboxMessages.Add(message);

        _logger.LogInformation("Outbox message queued. Type={Type} RoutingKey={RoutingKey} Id={Id}", message.Type, routingKey, message.Id);

        return Task.CompletedTask;
    }



}
