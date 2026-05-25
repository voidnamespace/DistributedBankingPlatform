namespace AccountService.Infrastructure.Messaging.Options;

public class SegmentationEventsConsumerOptions
{
    public string Exchange { get; init; } = default!;
    public string Queue { get; init; } = default!;
}
