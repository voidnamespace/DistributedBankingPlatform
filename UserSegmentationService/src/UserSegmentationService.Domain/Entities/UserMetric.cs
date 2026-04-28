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
}
