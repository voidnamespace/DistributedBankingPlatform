using System.Text.Json;
using UserSegmentationService.Application.Interfaces.Messaging;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly SegmentationDbContext _context;
    
    public OutboxWriter(SegmentationDbContext context)
    {
        _context = context;
    }

    public  Task EnqueueAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent == null)
        {
            throw new ArgumentNullException(nameof(integrationEvent));
        }

        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            IntegrationEventTypeMap.GetName(typeof(T)),
            JsonSerializer.Serialize(integrationEvent),
            DateTime.UtcNow);

        _context.OutboxMessages.Add(message);

        return Task.CompletedTask;
    }

}
