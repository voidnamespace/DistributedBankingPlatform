using TransactionService.Domain.Enums;
using TransactionService.Domain.Events;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Entities;

public class Transaction : Entity
{
    public Guid TransactionId { get; private set; }

    public AccountNumberVO FromAccountNumber { get; private set; } = default!;

    public AccountNumberVO ToAccountNumber { get; private set; } = default!;

    public MoneyVO Money { get; private set; } = null!;

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Transaction() { }

    public static Transaction CreateWithdrawal(
    AccountNumberVO fromAccountNumber,
    MoneyVO money)
    {
        var transaction = new Transaction();

        transaction.TransactionId = Guid.NewGuid();
        transaction.FromAccountNumber = fromAccountNumber;
        transaction.Money = money;
        transaction.Type = TransactionType.Withdraw;
        transaction.Status = TransactionStatus.Processing;
        transaction.CreatedAt = DateTime.UtcNow;

        transaction.AddDomainEvent(new WithdrawalCreatedDomainEvent(
            transaction.TransactionId, 
            transaction.FromAccountNumber, 
            transaction.Money));

        return transaction;
    }
    public static Transaction CreateDeposit(
        AccountNumberVO toAccountNumber,
        MoneyVO money)
    {
        var transaction = new Transaction();

        transaction.TransactionId = Guid.NewGuid();
        transaction.ToAccountNumber = toAccountNumber;
        transaction.Money = money;
        transaction.Type = TransactionType.Deposit;
        transaction.Status = TransactionStatus.Processing;
        transaction.CreatedAt = DateTime.UtcNow;

        transaction.AddDomainEvent(new DepositCreatedDomainEvent(
            transaction.TransactionId,
            transaction.ToAccountNumber,
            transaction.Money));

        return transaction;
    }



    public static Transaction CreateTransfer(
        AccountNumberVO fromAccountNumber,
        AccountNumberVO toAccountNumber,
        MoneyVO money)
    {
        if (fromAccountNumber == toAccountNumber)
            throw new DomainException("Accounts must be different");

        var transaction = new Transaction();

        transaction.TransactionId = Guid.NewGuid();
        transaction.FromAccountNumber = fromAccountNumber;
        transaction.ToAccountNumber = toAccountNumber;
        transaction.Money = money;
        transaction.Type = TransactionType.Transfer;
        transaction.Status = TransactionStatus.Processing;
        transaction.CreatedAt = DateTime.UtcNow;

        transaction.AddDomainEvent(new TransferCreatedDomainEvent(
            transaction.TransactionId,
            transaction.FromAccountNumber,
            transaction.ToAccountNumber,
            transaction.Money));

        return transaction;
    }




    public void StartProcessing()
    {
        if (Status != TransactionStatus.Processing)
            throw new DomainException("Only created transaction can be processed");

        Status = TransactionStatus.Processing;
    }

    public void Complete()
    {
        if (Status != TransactionStatus.Processing)
            throw new DomainException("Only processing transaction can be completed");

        Status = TransactionStatus.Completed;
    }

    public void Fail()
    {
        if (Status == TransactionStatus.Completed)
            throw new DomainException("Completed transaction cannot be failed");

        Status = TransactionStatus.Failed;
    }

}
