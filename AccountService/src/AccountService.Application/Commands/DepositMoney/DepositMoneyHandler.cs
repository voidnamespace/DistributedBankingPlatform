using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;

namespace AccountService.Application.Commands.DepositMoney;

public class DepositMoneyHandler : IRequestHandler<DepositMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositMoneyHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle (DepositMoneyCommand command, CancellationToken ct)
    {
        var accNum = new AccountNumberVO(command.AccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct)
            ?? throw new KeyNotFoundException("No such acc");
        var money = new MoneyVO(command.request.Amount, command.request.Currency);
        acc.Deposit(money);
        await _unitOfWork.SaveChangesAsync(ct);

    }

}
