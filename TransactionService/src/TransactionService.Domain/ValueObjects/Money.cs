using TransactionService.Domain.Exceptions;
using TransactionService.Domain.Enums;
namespace TransactionService.Domain.ValueObjects;

public sealed class MoneyVO : IEquatable<MoneyVO>
{
    public decimal Amount { get; }
    public Currency Currency { get; } 

    private MoneyVO() { }
    public MoneyVO(decimal amount, Currency currency)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        Amount = amount;
        Currency = currency;
    }

    public bool Equals(MoneyVO? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
        => Equals(obj as MoneyVO);

    public override int GetHashCode()
        => HashCode.Combine(Amount, Currency);

    public override string ToString()
        => $"{Amount} {Currency}";
}
