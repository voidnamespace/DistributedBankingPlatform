using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Data;
namespace TransactionService.Infrastructure.Persistence.Inbox;

public class InboxWriter : IInboxWriter
{

    private readonly TransactionDbContext _context;
    private readonly ILogger<InboxWriter> _logger;

    public InboxWriter (TransactionDbContext context,
        ILogger<InboxWriter> logger)
    {
        _context = context; 
        _logger = logger;
    }

    public async Task SaveAsync(Guid messageId, 
        string type,
        string payload,
        CancellationToken ct)
    {
        var exists = await _context.InboxMessages
                    .AnyAsync(x => x.Id == messageId, ct);

        if (exists)
        {
            _logger.LogInformation("Duplicate message skipped: {MessageId}", messageId);
            return;
        }


        var inboxMessage = new InboxMessage
        {
            Id = messageId,
            Type = type,
            Payload = payload,
            Processed = false,
            AttemptCount = 0,
            ReceivedAt = DateTime.UtcNow,
        };
        
        _context.InboxMessages.Add(inboxMessage);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Inbox message saved. Id={Id} Type={Type}",
            inboxMessage.Id,
            type);

    }
}
