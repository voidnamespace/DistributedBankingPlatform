namespace UserSegmentationService.Application.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(
        object message,
        CancellationToken ct);
}
