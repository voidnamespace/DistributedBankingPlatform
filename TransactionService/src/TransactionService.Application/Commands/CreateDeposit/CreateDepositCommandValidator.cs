using FluentValidation;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Commands.CreateDeposit;

public class CreateDepositCommandValidator 
    : AbstractValidator<CreateDepositCommand>
{
    public CreateDepositCommandValidator()
    {
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
