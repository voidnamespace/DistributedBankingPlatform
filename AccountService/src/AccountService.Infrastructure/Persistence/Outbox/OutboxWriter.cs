using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Data;
using System.Text.Json;
namespace AccountService.Infrastructure.Persistence.Outbox;

public class OutboxWriter : IOutboxWriter
{
    private readonly AccountDbContext _context;

    public OutboxWriter(AccountDbContext context)
    {
        _context = context;
    }

    public Task EnqueueAsync<T>(
        T integrationEvent,
        CancellationToken ct)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).FullName!,
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc = DateTime.UtcNow,
            AttemptCount = 0
        };

        _context.OutboxMessages.Add(message);

        return Task.CompletedTask;
    }
}
