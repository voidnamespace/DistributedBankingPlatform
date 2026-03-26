
namespace EventProjectionService.Infrastructure.Messaging.Options;

public sealed class ProjectionEventsConsumerOptions
{
    public string Queue { get; init; } = "projection.events";
}
