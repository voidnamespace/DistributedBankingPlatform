namespace AccountService.Application.Interfaces.Messaging;

public interface IInboxWriter
{
    Task SaveAsync(
        string type,
        string payload,
        string routingKey,
        CancellationToken ct);
}