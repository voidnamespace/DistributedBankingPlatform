namespace AuthService.Application.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T message,
        CancellationToken ct = default);

}
