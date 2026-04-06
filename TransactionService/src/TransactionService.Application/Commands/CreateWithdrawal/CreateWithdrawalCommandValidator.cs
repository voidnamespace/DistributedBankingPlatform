using FluentValidation;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandValidator 
    : AbstractValidator<CreateWithdrawalCommand>
{
    public CreateWithdrawalCommandValidator()
    {
        RuleFor(x => x.InitiatorId)
            .NotEmpty();

        RuleFor(x => x.FromAccountNumber)
            .NotEmpty()
            .Length(12);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .Must(value => Enum.IsDefined(typeof(Currency), value))
            .WithMessage("Invalid currency");
    }
}
