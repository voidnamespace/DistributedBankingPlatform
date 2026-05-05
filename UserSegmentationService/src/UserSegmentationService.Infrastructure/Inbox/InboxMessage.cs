namespace UserSegmentationService.Infrastructure.Inbox;

public class InboxMessage
{
    private InboxMessage()
    {
    }

    private InboxMessage(
        Guid messageId,
        string type,
        string payload,
        DateTime receivedAt)
    {
        MessageId = messageId;
        Type = type;
        Payload = payload;
        ReceivedAt = receivedAt;
    }

    public Guid MessageId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTime ReceivedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public static InboxMessage Create(
        Guid messageId,
        string type,
        string payload,
        DateTime receivedAt)
    {
        return new InboxMessage(messageId, type, payload, receivedAt);
    }

    public void MarkProcessed(DateTime processedAt)
    {
        ProcessedAt = processedAt;
        LastError = null;
        NextAttemptAt = null;
    }

    public void MarkFailed(
        string error,
        DateTime failedAt)
    {
        AttemptCount++;
        LastError = error;
        NextAttemptAt = failedAt.AddSeconds(GetRetryDelaySeconds());
    }

    private int GetRetryDelaySeconds()
    {
        var delay = Math.Pow(2, AttemptCount) * 5;

        return (int)Math.Min(delay, 300);
    }
}
