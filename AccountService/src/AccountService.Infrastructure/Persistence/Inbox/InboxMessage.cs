namespace AccountService.Infrastructure.Persistence.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public bool Processed { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? Error { get; set; }
}
