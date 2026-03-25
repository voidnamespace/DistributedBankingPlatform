using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Messaging.Routing;
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

    public Task EnqueueAsync<T>(T integrationEvent, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = IntegrationEventMap.GetName(typeof(T)),
            Payload = JsonSerializer.Serialize(integrationEvent),
            CreatedAt = DateTime.UtcNow,
            AttemptCount = 0,
        };

        _context.OutboxMessages.Add(message);

        _logger.LogInformation("Outbox message queued. Type={Type} Id={Id}", message.Type, message.Id);

        return Task.CompletedTask;
    }



}
