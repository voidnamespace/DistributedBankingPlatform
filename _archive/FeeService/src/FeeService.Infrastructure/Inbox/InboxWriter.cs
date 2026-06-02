using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeeService.Infrastructure.Inbox;

public sealed class InboxWriter : IInboxWriter
{
    private readonly FeeDbContext _context;
    private readonly ILogger<InboxWriter> _logger;
    public InboxWriter(
        FeeDbContext context,
        ILogger<InboxWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveAsync(
        Guid messageId,
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        var alreadyExists = await _context.InboxMessages
            .AnyAsync(x => x.Id == messageId, cancellationToken);

        if (alreadyExists)
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
            ReceivedAt = DateTime.UtcNow,
            AttemptCount = 0,
            Error = null,
            ProcessedAt = null
        };

         _context.InboxMessages.Add(inboxMessage);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inbox message saved. Id={Id} Type={Type} ",
            inboxMessage.Id,
            type);
    }
}
