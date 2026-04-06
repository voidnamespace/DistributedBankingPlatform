using FluentValidation;

namespace TransactionService.Application.Commands.CreateDeposit;

public class CreateDepositCommandValidator 
    : AbstractValidator<CreateDepositCommand>
{
    public CreateDepositCommandValidator()
    {

    }
}
