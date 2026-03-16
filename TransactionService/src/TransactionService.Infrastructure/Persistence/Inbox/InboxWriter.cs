using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
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
        string routingKey,
        CancellationToken ct)
    {



        var inboxMessage = new InboxMessage
        {
            Id = messageId,
            Type = type,
            Payload = payload,
            RoutingKey = routingKey,
            Processed = false,
            AttemptCount = 0,
            ReceivedAt = DateTime.UtcNow,
        };

        _context.InboxMessages.Add(inboxMessage);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Inbox message saved. Id={Id} Type={Type} RoutingKey={RoutingKey}",
            inboxMessage.Id,
            type,
            routingKey);


    }





}
