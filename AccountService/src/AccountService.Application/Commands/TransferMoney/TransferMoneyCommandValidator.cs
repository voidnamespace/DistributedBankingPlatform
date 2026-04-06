using AccountService.Domain.Enums;
using FluentValidation;

namespace AccountService.Application.Commands.TransferMoney;

public class TransferMoneyCommandValidator
    : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.InitiatorId)
            .NotEmpty();

        RuleFor(x => x.TransactionId)
            .NotEmpty();

        RuleFor(x => x.FromAccountNumber)
            .NotEmpty()
            .Length(12);

        RuleFor(x => x.ToAccountNumber)
            .NotEmpty()
            .Length(12)
            .NotEqual(x => x.FromAccountNumber);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .Must(value => Enum.IsDefined(typeof(Currency), value))
            .WithMessage("Invalid currency");
    }
}
