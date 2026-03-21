namespace AuthService.Application.Interfaces.Messaging;

public interface IOutboxWriter
{
    Task EnqueueAsync<T>(
        T integrationEvent,
        CancellationToken ct);
}