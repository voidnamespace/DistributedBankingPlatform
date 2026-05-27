using UserSegmentationService.Domain.Enums;
using UserSegmentationService.Domain.ValueObjects;

namespace UserSegmentationService.Domain.Entities;

public class UserMetric
{
    public Guid UserId { get; private set; }

    public MoneyVO SpendLast60Days { get; private set; } = default!;

    public DateTime? LastTransactionAt { get; private set; }

    private UserMetric() { }

    public UserMetric(Guid userId)
    {
        UserId = userId;
        SpendLast60Days = new MoneyVO(0, Currency.Copper);
    }

    public static UserMetric CreateSnapshot(
        Guid userId,
        decimal spendLast60DaysInCopper,
        DateTime? lastTransactionAt)
    {
        return new UserMetric
        {
            UserId = userId,
            SpendLast60Days = new MoneyVO(spendLast60DaysInCopper, Currency.Copper),
            LastTransactionAt = lastTransactionAt
        };
    }

    public void RecordTransaction(DateTime occurredAt)
    {
        LastTransactionAt = LastTransactionAt is null || occurredAt > LastTransactionAt
            ? occurredAt
            : LastTransactionAt;
    }

    public void RecordSpend(
        decimal amountInCopper,
        DateTime occurredAt)
    {
        if (amountInCopper <= 0)
            return;

        SpendLast60Days = new MoneyVO(
            SpendLast60Days.Amount + amountInCopper,
            Currency.Copper);

        RecordTransaction(occurredAt);
    }
}
