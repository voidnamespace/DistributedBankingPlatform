using AccountService.Domain.Enums;
using FluentValidation;

namespace AccountService.Application.Commands.DepositMoney;

public class DepositMoneyCommandValidator
    : AbstractValidator<DepositMoneyCommand>
{
    public DepositMoneyCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty();

        RuleFor(x => x.ToAccountNumber)
            .NotEmpty()
            .Length(12);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .Must(value => Enum.IsDefined(typeof(Currency), value))
            .WithMessage("Invalid currency");
    }
}
