namespace FeeService.Infrastructure.Persistence.Inbox;

public class DeadLetterInboxMessage
{
    public Guid MessageId { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime ReceivedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? Error { get; set; }

    public DateTime DeadLetteredAt { get; set; }
}
