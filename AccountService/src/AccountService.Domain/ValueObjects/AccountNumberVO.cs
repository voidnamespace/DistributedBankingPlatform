using AccountService.Domain.Exceptions;

namespace AccountService.Domain.ValueObjects;

public sealed class AccountNumberVO
{
    private readonly string _value;

    private AccountNumberVO()
    {
        _value = string.Empty; 
    }

    public string Value => _value;

    public AccountNumberVO(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new DomainException("Account number is required");

        if (accountNumber.Length != 12)
            throw new DomainException("Account number must be exactly 12 digits");

        if (!accountNumber.All(char.IsDigit))
            throw new DomainException("Account number must contain only digits");

        _value = accountNumber;
    }
    public static AccountNumberVO Generate()
    {
        var value = string.Concat(
            Enumerable.Range(0, 12)
                .Select(_ => Random.Shared.Next(0, 10))
        );

        return new AccountNumberVO(value);
    }


    public override string ToString() => _value;
}
