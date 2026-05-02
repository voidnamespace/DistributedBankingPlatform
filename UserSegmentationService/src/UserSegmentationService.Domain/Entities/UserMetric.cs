namespace UserSegmentationService.Domain.Entities;

public class UserMetric
{
    public Guid UserId { get; private set; }

    public decimal SpendLast60Days { get; private set; }

    public DateTime? LastTransactionAt { get; private set; }

    private UserMetric() { }

    public UserMetric(Guid userId)
    {
        UserId = userId;
    }

    public void RecordSpend(
        decimal amount,
        DateTime occurredAt)
    {
        if (amount <= 0)
            return;

        SpendLast60Days += amount;
        LastTransactionAt = LastTransactionAt is null || occurredAt > LastTransactionAt
            ? occurredAt
            : LastTransactionAt;
    }
}
