using TransactionService.Domain.Enums;
using TransactionService.Domain.Events;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Entities;

public class Transaction : Entity
{
    public Guid TransactionId { get; private set; }

    public string FromAccountId { get; private set; } = default!;

    public string ToAccountId { get; private set; } = default!;

    public MoneyVO Money { get; private set; } = null!;

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Transaction() { }


    public Transaction (string fromAccountNumber, string toAccountNumber, MoneyVO money)
    {
        if (fromAccountNumber == toAccountNumber)
            throw new DomainException("Accounts must be different");

        TransactionId = Guid.NewGuid();
        FromAccountId = fromAccountNumber;
        ToAccountId = toAccountNumber;
        Money = money;
        Status = TransactionStatus.Processing;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransferCreatedDomainEvent(TransactionId, fromAccountNumber, toAccountNumber, money));

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
