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

    public static UserMetric CreateSnapshot(
        Guid userId,
        decimal spendLast60Days,
        DateTime? lastTransactionAt)
    {
        if (spendLast60Days < 0)
            throw new ArgumentOutOfRangeException(
                nameof(spendLast60Days),
                "Spend last 60 days cannot be negative.");

        return new UserMetric
        {
            UserId = userId,
            SpendLast60Days = spendLast60Days,
            LastTransactionAt = lastTransactionAt
        };
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
