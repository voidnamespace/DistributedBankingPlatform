namespace AccountService.Infrastructure.Persistence.Outbox;

public class DeadLetterOutboxMessage
{
    public Guid Id { get; private set; }

    public Guid MessageId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public int AttemptCount { get; private set; }

    public string Error { get; private set; } = string.Empty;

    public DateTime FailedAt { get; private set; }

    private DeadLetterOutboxMessage()
    {
    }

    private DeadLetterOutboxMessage(
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

    public static DeadLetterOutboxMessage From(
    OutboxMessage message,
    string error,
    DateTime failedAt)
    {
        return new DeadLetterOutboxMessage(
            Guid.NewGuid(),
            message.Id,
            message.Type,
            message.Payload,
            message.AttemptCount,
            error,
            failedAt);
    }

}
