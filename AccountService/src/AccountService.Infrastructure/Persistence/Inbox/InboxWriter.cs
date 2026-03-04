using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Persistence.Inbox;

public class InboxWriter : IInboxWriter
{
    private readonly AccountDbContext _context;

    public InboxWriter(AccountDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        string type,
        string payload,
        string routingKey,
        CancellationToken ct)
    {
        var message = new InboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            RoutingKey = routingKey,
            ReceivedAt = DateTime.UtcNow,
            AttemptCount = 0,
            Processed = false
        };

        _context.InboxMessages.Add(message);
        await _context.SaveChangesAsync(ct);
    }
}