using TransactionService.Domain.Enums;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }

    public Guid CardId { get; private set; }

    public Guid FromAccountId { get; private set; }

    public Guid ToAccountId { get; private set; }

    public MoneyVO Money { get; private set; } = null!;

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Transaction() { }


    public Transaction (Guid cardId, Guid fromAccountId, Guid toAccountId, MoneyVO money)
    {
        if (fromAccountId == toAccountId)
            throw new DomainException("Accounts must be different");

        Id = Guid.NewGuid();
        CardId = cardId;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Money = money;
        Status = TransactionStatus.Created;
        CreatedAt = DateTime.UtcNow;

    }

    public void StartProcessing()
    {
        if (Status != TransactionStatus.Created)
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
