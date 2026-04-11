using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Persistence.Inbox;

public sealed class InboxWriter : IInboxWriter
{
    private readonly FeeDbContext _dbContext;

    public InboxWriter(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        Guid messageId,
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        var alreadyExists = await _dbContext.InboxMessages
            .AnyAsync(x => x.Id == messageId, cancellationToken);

        if (alreadyExists)
            return;

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

        await _dbContext.InboxMessages.AddAsync(inboxMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
