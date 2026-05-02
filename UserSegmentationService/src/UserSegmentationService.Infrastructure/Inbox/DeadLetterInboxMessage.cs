namespace UserSegmentationService.Infrastructure.Inbox;

public class DeadLetterInboxMessage
{
    private DeadLetterInboxMessage()
    {
    }

    private DeadLetterInboxMessage(
        Guid id,
        Guid messageId,
        string type,
        string payload,
        int attemptCount,
        string error,
        DateTime failedAt)
    {
        Id = id;
        MessageId = messageId;
        Type = type;
        Payload = payload;
        AttemptCount = attemptCount;
        Error = error;
        FailedAt = failedAt;
    }

    public Guid Id { get; private set; }

    public Guid MessageId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public int AttemptCount { get; private set; }

    public string Error { get; private set; } = string.Empty;

    public DateTime FailedAt { get; private set; }

    public static DeadLetterInboxMessage From(
        InboxMessage message,
        string error,
        DateTime failedAt)
    {
        return new DeadLetterInboxMessage(
            Guid.NewGuid(),
            message.MessageId,
            message.Type,
            message.Payload,
            message.AttemptCount,
            error,
            failedAt);
    }
}
