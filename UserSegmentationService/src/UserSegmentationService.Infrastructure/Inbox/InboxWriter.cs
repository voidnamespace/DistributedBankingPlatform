using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Inbox;

internal class InboxWriter : IInboxWriter
{
    private readonly SegmentationDbContext _dbContext;

    public InboxWriter(SegmentationDbContext dbContext)
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
            .AnyAsync(x => x.MessageId == messageId, cancellationToken);

        if (alreadyExists)
            return;

        var message = InboxMessage.Create(
            messageId,
            type,
            payload,
            DateTime.UtcNow);

        _dbContext.InboxMessages.Add(message);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
