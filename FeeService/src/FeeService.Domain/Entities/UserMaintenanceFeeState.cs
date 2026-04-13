namespace FeeService.Domain.Entities;

public class UserMaintenanceFeeState : Entity
{
    public Guid UserId { get; private set; }

    public DateTime? ChargedAt { get; private set; }

    public DateTime NextChargeAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private UserMaintenanceFeeState() { }

    private UserMaintenanceFeeState(Guid userId)
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ChargedAt = null;
        NextChargeAt = DateTime.UtcNow.AddMonths(1);
    }


    public static UserMaintenanceFeeState Create(Guid userId)
    {
        return new UserMaintenanceFeeState(userId);

    }

    public void MarkCharged(DateTime now)
    {
        ChargedAt = now;

        NextChargeAt = now.AddMonths(1);
    }
}
