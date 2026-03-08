using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AccountService.Application.Commands.WithdrawMoney;

public class WithdrawMoneyHandler : IRequestHandler<WithdrawMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WithdrawMoneyHandler> _logger;

    public WithdrawMoneyHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<WithdrawMoneyHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WithdrawMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawMoneyCommand received for account {AccountNumber}, amount {Amount} {Currency}",
            command.AccountNumber,
            command.Amount,
            command.Currency);

        var accNum = new AccountNumberVO(command.AccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct)
            ?? throw new KeyNotFoundException("No such acc");

        if (acc.UserId != command.UserId)
            throw new InvalidOperationException("U can withdrawal only your account");

        var money = new MoneyVO(
            command.Amount,
            command.Currency
        );

        acc.Withdraw(money);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Withdraw completed for account {AccountNumber}, amount {Amount} {Currency}",
            command.AccountNumber,
            command.Amount,
            command.Currency);
    }
}