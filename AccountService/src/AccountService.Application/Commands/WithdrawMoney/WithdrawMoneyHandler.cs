using AccountService.Application.IntegrationEvents.Transactions.Withdrawal;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.WithdrawMoney;

public class WithdrawMoneyHandler : IRequestHandler<WithdrawMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WithdrawMoneyHandler> _logger;
    private readonly IOutboxWriter _outboxWriter;

    public WithdrawMoneyHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<WithdrawMoneyHandler> logger,
        IOutboxWriter outboxWriter)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(WithdrawMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawMoneyCommand received for account {AccountNumber}, amount {Amount} {Currency}",
            command.FromAccountNumber,
            command.Amount,
            command.Currency);

        var accNum = new AccountNumberVO(command.FromAccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct)
            ?? throw new KeyNotFoundException("No such acc");

        var money = new MoneyVO(
            command.Amount,
            (Currency)command.Currency
        );

        if (command.Amount > acc.Balance.Amount)
        {
            await _outboxWriter.EnqueueAsync(new WithdrawalFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountNumber,
                command.Amount,
                command.Currency), ct);

            return;
        }

        acc.Withdraw(money);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Withdraw completed for account {AccountNumber}, amount {Amount} {Currency}",
            command.FromAccountNumber,
            command.Amount,
            command.Currency);
    }
}
