namespace AccountService.Application.Interfaces.Messaging;

public interface IInboxWriter
{
    Task SaveAsync(Guid messageId,
        string type,
        string payload,
        CancellationToken ct);
}