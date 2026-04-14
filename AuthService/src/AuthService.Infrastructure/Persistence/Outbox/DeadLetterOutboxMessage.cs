namespace AuthService.Infrastructure.Persistence.Outbox;

public class DeadLetterOutboxMessage
{
    public Guid Id { get; set; }

    public Guid OriginalOutboxMessageId { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public string Error { get; set; } = default!;

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime FinalFailedAt { get; set; }
}
