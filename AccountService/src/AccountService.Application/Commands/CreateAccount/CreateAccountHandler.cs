using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
using MediatR;
namespace AccountService.Application.Commands.CreateAccount;

public class CreateAccountHandler
    : IRequestHandler<CreateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CreateAccountCommand command,
        CancellationToken ct)
    {
        AccountNumberVO accountNumberVO;

        do
        {
            accountNumberVO = AccountNumberVO.Generate();
        }
        while (await _accountRepository
            .ExistsByAccountNumberAsync(accountNumberVO, ct));

        var acc = new Account(
            command.UserId,
            accountNumberVO,
            command.Currency);

        await _accountRepository.AddAsync(acc, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
