using AccountService.Application.Interfaces;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AccountService.Application.Commands.DepositMoney;

public class DepositMoneyHandler : IRequestHandler<DepositMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepositMoneyHandler> _logger;

    public DepositMoneyHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DepositMoneyHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DepositMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "DepositMoneyCommand received for account {AccountNumber}, amount {Amount} {Currency}",
            command.ToAccountNumber,
            command.Amount,
            command.Currency);

        var accNum = new AccountNumberVO(command.ToAccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct)
            ?? throw new KeyNotFoundException("No such acc");

        var money = new MoneyVO(
            command.Amount,
            (Currency)command.Currency
        );




        acc.Deposit(money, command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deposit completed for account {AccountNumber}, amount {Amount} {Currency}",
            command.ToAccountNumber,
            command.Amount,
            command.Currency);
    }
}