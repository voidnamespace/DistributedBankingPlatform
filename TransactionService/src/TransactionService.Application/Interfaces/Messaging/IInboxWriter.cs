namespace TransactionService.Application.Interfaces.Messaging;

public interface IInboxWriter
{
    Task SaveAsync(Guid messageId,
        string type,
        string payload,
        string routingKey,
        CancellationToken ct);
}
