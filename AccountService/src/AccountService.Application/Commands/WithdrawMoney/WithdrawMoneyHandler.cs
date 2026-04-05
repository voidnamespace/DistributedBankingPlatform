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
    private readonly ILogger<WithdrawMoneyHandler> _logger;
    private readonly IOutboxWriter _outboxWriter;

    public WithdrawMoneyHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<WithdrawMoneyHandler> logger,
        IOutboxWriter outboxWriter)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _outboxWriter = outboxWriter;
    }

    private async Task PublishWithdrawalFailed(
    WithdrawMoneyCommand command,
    string reason,
    CancellationToken ct)
    {
        await _outboxWriter.EnqueueAsync(
            new WithdrawalFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountNumber,
                command.Amount,
                command.Currency,
                reason
            ),
            ct);
    }

    public async Task Handle(WithdrawMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawMoneyCommand received for account {AccountNumber}, amount {Amount} {Currency}",
            command.FromAccountNumber,
            command.Amount,
            command.Currency);

        var accNum = new AccountNumberVO(command.FromAccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct);
            
        if ( acc == null )
        {
            await PublishWithdrawalFailed(command,
                "FromAccountNotFound",
                ct);
            return;
        }

        if (command.Amount <= 0)
        {
            await PublishWithdrawalFailed(command,
                "InvalidAmount",
                ct);
            return;
        }

        if (command.InitiatorId != acc.UserId)
        {
            await PublishWithdrawalFailed(command,
                "OwnershipMismatch",
                ct);
            return;
        }

        if ((Currency)command.Currency != acc.Balance.Currency)
        {
            await PublishWithdrawalFailed(command,
                "InvalidCurrency",
                ct);
            return;

        }

        if (command.Amount > acc.Balance.Amount)
        {
            await PublishWithdrawalFailed(command,
                  "InvalidAmount",
                  ct);
            return;
        }

        var money = new MoneyVO(
            command.Amount,
            (Currency)command.Currency
        );

        acc.Withdraw(money, command.TransactionId);

        _logger.LogInformation(
            "Withdraw completed for account {AccountNumber}, amount {Amount} {Currency}",
            command.FromAccountNumber,
            command.Amount,
            command.Currency);
    }
}
