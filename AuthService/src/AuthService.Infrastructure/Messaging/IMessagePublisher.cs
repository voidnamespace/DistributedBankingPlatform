namespace AuthService.Infrastructure.Messaging;

internal interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        string? messageId = null,
        CancellationToken cancellationToken = default);
}
