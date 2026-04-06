using FluentValidation;

namespace TransactionService.Application.Commands.CreateTransfer;

public class CreateTransferCommandValidator
    : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.FromAccountNumber)
            .NotEmpty()
            .WithMessage("FromAccountNumber is required");

        RuleFor(x => x.ToAccountNumber)
            .NotEmpty()
            .WithMessage("ToAccountNumber is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .IsInEnum()
            .WithMessage("Invalid currency");

        RuleFor(x => x)
            .Must(x => x.FromAccountNumber != x.ToAccountNumber)
            .WithMessage("Cannot transfer to the same account");
    }
}
