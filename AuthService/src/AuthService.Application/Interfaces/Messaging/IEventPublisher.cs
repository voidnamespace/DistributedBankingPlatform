namespace AuthService.Application.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T message,
        string? messageId = null,
        CancellationToken ct = default);

}
