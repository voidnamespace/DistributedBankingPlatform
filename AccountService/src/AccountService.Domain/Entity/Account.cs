using AccountService.Domain.ValueObjects;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Enums;
using AccountService.Domain.Events;

namespace AccountService.Domain.Entity;

public class Account : Entity
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


    public void TransferTo(Account toAccount, MoneyVO money, Guid transactionId)
    {
        if (toAccount == null)
            throw new DomainException("Target account is null");

        if (Id == toAccount.Id)
            throw new DomainException("Cannot transfer to same account");
        if (money.Amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        if (!IsActive)
        {
            AddDomainEvent(new TransferFailedDomainEvent(transactionId,
            AccountNumber,
            toAccount.AccountNumber,
            money
            
        ));
            return;
        }
        if (!toAccount.IsActive)
        {
            AddDomainEvent(new TransferFailedDomainEvent(transactionId,
                AccountNumber,
                toAccount.AccountNumber,
                money
            ));
            return;
        }
        if (Balance.Amount < money.Amount)
        {
            AddDomainEvent(new TransferFailedDomainEvent(transactionId,
            AccountNumber,
            toAccount.AccountNumber,
            money
        ));
            return;
        }
        if(Balance.Currency != money.Currency)
        {
            AddDomainEvent(new TransferFailedDomainEvent(transactionId,
            AccountNumber,
            toAccount.AccountNumber,
            money
        ));
            return;
        }

        DecreaseBalance(money);
        toAccount.IncreaseBalance(money);

        AddDomainEvent(new TransferSuccessDomainEvent(transactionId,
            AccountNumber,
            toAccount.AccountNumber,
            money
        ));
    }

    private void IncreaseBalance(MoneyVO money)
    {
        Balance = new MoneyVO(
            Balance.Amount + money.Amount,
            Balance.Currency
        );

        UpdatedAt = DateTime.UtcNow;
    }

    private void DecreaseBalance(MoneyVO money)
    {
        Balance = new MoneyVO(
            Balance.Amount - money.Amount,
            Balance.Currency
        );

        UpdatedAt = DateTime.UtcNow;
    }

}
