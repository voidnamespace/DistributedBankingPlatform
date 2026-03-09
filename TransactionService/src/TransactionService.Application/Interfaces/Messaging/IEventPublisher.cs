namespace TransactionService.Application.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken ct = default);

    Task PublishRawAsync(string payloadJson, string routingKey, CancellationToken ct);
}
