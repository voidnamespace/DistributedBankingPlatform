using AccountService.Domain.ValueObjects;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Enums;
namespace AccountService.Domain.Entity;

public class Account
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public AccountNumberVO AccountNumber { get; private set; } = default!;
    public MoneyVO Balance { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    private Account() { }
    public Account (Guid userId, AccountNumberVO accountNumberVO, Currency currency)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AccountNumber = accountNumberVO;
        Balance = new MoneyVO(0, currency); 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Withdraw(MoneyVO moneyVO)
    {
        if (!IsActive)
            throw new DomainException("Account is inactive");

        if (moneyVO.Currency != Balance.Currency)
            throw new DomainException("Currency mismatch");

        if (moneyVO.Amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        if (moneyVO.Amount > Balance.Amount)
            throw new DomainException("Insufficient balance");

        Balance = new MoneyVO(
            Balance.Amount - moneyVO.Amount,
            Balance.Currency
        );

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deposit(MoneyVO moneyVO)
    {
        if (!IsActive)
            throw new DomainException("Account is inactive");

        if (moneyVO.Currency != Balance.Currency)
            throw new DomainException("Currency mismatch");

        if (moneyVO.Amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        Balance = new MoneyVO(
            Balance.Amount + moneyVO.Amount, Balance.Currency
            );
        UpdatedAt = DateTime.UtcNow;
    }

}
