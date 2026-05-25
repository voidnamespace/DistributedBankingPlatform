namespace UserSegmentationService.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid MessageId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTime OccurredAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    private OutboxMessage()
    {

    }

    private OutboxMessage(
        Guid messageId,
        string type,
        string payload,
        DateTime occurredAt)
    {
        MessageId = messageId;
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
    }
    
    public static OutboxMessage Create(
        Guid messageId,
        string type,
        string payload,
        DateTime occuredAt)
    {
        return new OutboxMessage(messageId, type, payload, occuredAt);
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
