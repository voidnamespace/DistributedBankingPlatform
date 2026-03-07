using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;
namespace AccountService.Application.Commands.WithdrawMoney;

public class WithdrawMoneyHandler : IRequestHandler<WithdrawMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawMoneyHandler (IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository; 
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WithdrawMoneyCommand command, CancellationToken ct)
    {
        var accNum = new AccountNumberVO(command.AccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct)
            ?? throw new KeyNotFoundException("No such acc");

        if (acc.UserId!=command.UserId)
            throw new InvalidOperationException("U can withdrawal only your account");

        var money = new MoneyVO(
            command.Amount,
            command.Currency
        );

        acc.Withdraw(money);

        await _unitOfWork.SaveChangesAsync(ct);
    }

}
