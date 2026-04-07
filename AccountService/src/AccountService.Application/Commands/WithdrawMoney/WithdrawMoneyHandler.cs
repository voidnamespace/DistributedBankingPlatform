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

        _logger.LogInformation(
            "WithdrawalFailedIntegrationEvent queued for transaction {TransactionId}, reason {Reason}",
            command.TransactionId,
            reason);
    }


    public async Task Handle(WithdrawMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "WithdrawMoneyCommand received for transaction {TransactionId}, account {AccountNumber}, amount {Amount} {Currency}",
            command.TransactionId,
            command.FromAccountNumber,
            command.Amount,
            command.Currency);


        var accNum = new AccountNumberVO(command.FromAccountNumber);

        var acc = await _accountRepository.GetByAccountNumberAsync(accNum, ct);


        if (acc == null)
        {
            _logger.LogWarning(
                "WithdrawMoneyCommand failed for transaction {TransactionId}: account {AccountNumber} not found",
                command.TransactionId,
                command.FromAccountNumber);

            await PublishWithdrawalFailed(command, "FromAccountNotFound", ct);
            return;
        }


        if (command.Amount <= 0)
        {
            _logger.LogWarning(
                "WithdrawMoneyCommand failed for transaction {TransactionId}: invalid amount {Amount}",
                command.TransactionId,
                command.Amount);

            await PublishWithdrawalFailed(command, "InvalidAmount", ct);
            return;
        }


        if (command.InitiatorId != acc.UserId)
        {
            _logger.LogWarning(
                "WithdrawMoneyCommand failed for transaction {TransactionId}: ownership mismatch for account {AccountNumber}",
                command.TransactionId,
                command.FromAccountNumber);

            await PublishWithdrawalFailed(command, "OwnershipMismatch", ct);
            return;
        }


        if ((Currency)command.Currency != acc.Balance.Currency)
        {
            _logger.LogWarning(
                "WithdrawMoneyCommand failed for transaction {TransactionId}: currency mismatch. Expected {ExpectedCurrency}, received {Currency}",
                command.TransactionId,
                acc.Balance.Currency,
                command.Currency);

            await PublishWithdrawalFailed(command, "InvalidCurrency", ct);
            return;
        }


        if (command.Amount > acc.Balance.Amount)
        {
            _logger.LogWarning(
                "WithdrawMoneyCommand failed for transaction {TransactionId}: insufficient funds. Balance {Balance}, requested {Amount}",
                command.TransactionId,
                acc.Balance.Amount,
                command.Amount);

            await PublishWithdrawalFailed(command, "InsufficientFunds", ct);
            return;
        }


        var money = new MoneyVO(
            command.Amount,
            (Currency)command.Currency
        );


        acc.Withdraw(money, command.TransactionId);


        _logger.LogInformation(
            "WithdrawMoneyCommand applied successfully for transaction {TransactionId}, account {AccountNumber}, amount {Amount} {Currency}",
            command.TransactionId,
            command.FromAccountNumber,
            command.Amount,
            command.Currency);
    }
}
