namespace UserSegmentationService.Domain.Entities;

public class UserAccount
{
    private UserAccount()
    {
    }

    public UserAccount(
        Guid accountId,
        Guid userId,
        string accountNumber)
    {
        AccountId = accountId;
        UserId = userId;
        AccountNumber = accountNumber;
    }

    public Guid AccountId { get; private set; }

    public Guid UserId { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;
}
