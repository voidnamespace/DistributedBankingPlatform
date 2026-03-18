using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
public class InboxWriter : IInboxWriter
{
    private readonly AccountDbContext _context;
    private readonly ILogger<InboxWriter> _logger;

    public InboxWriter(
        AccountDbContext context,
        ILogger<InboxWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveAsync(Guid messageId,
        string type,
        string payload,
        string routingKey,
        CancellationToken ct)
    {
        var exists = await _context.InboxMessages
                    .AnyAsync(x => x.Id == messageId, ct);

        if (exists)
        {
            _logger.LogInformation("Duplicate message skipped: {MessageId}", message.Id);
            return;
        }

        var message = new InboxMessage
        {
            Id = messageId,
            Type = type,
            Payload = payload,
            RoutingKey = routingKey,
            ReceivedAt = DateTime.UtcNow,
            AttemptCount = 0,
            Processed = false
        };
        
        _context.InboxMessages.Add(message);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Inbox message saved. Id={Id} Type={Type} RoutingKey={RoutingKey}",
            message.Id,
            type,
            routingKey);
    }
}