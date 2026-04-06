using FluentValidation;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandValidator 
    : AbstractValidator<CreateWithdrawalCommand>
{
    public CreateWithdrawalCommandValidator()
    {

    }
}
