namespace AuthService.Application.Interfaces.Messaging;

public interface IOutboxWriter
{
    Task EnqueueAsync<T>(
        T integrationEvent,
        string routingKey,
        CancellationToken ct);
}