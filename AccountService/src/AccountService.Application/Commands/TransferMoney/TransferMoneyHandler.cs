using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.TransferMoney;

public class TransferMoneyHandler : IRequestHandler<TransferMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<TransferMoneyHandler> _logger;

    public TransferMoneyHandler(
        IAccountRepository accountRepository,
        IOutboxWriter outboxWriter,
        ILogger<TransferMoneyHandler> logger)
    {
        _accountRepository = accountRepository;
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    private async Task PublishTransferFailed(
        TransferMoneyCommand command,
        string reason,
        CancellationToken ct)
    {
        await _outboxWriter.EnqueueAsync(
            new TransferFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountNumber,
                command.ToAccountNumber,
                command.Amount,
                command.Currency,
                reason
            ),
            ct);

        _logger.LogInformation(
            "TransferFailedIntegrationEvent queued for transaction {TransactionId}, reason {Reason}",
            command.TransactionId,
            reason);
    }

    public async Task Handle(TransferMoneyCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "TransferMoneyCommand received for transaction {TransactionId}, from {FromAccountNumber} to {ToAccountNumber}, amount {Amount} {Currency}",
            command.TransactionId,
            command.FromAccountNumber,
            command.ToAccountNumber,
            command.Amount,
            command.Currency);


        if (command.FromAccountNumber == command.ToAccountNumber)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: same account transfer attempted",
                command.TransactionId);

            await PublishTransferFailed(command, "SameAccountTransfer", ct);
            return;
        }


        if (command.Amount <= 0)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: invalid amount {Amount}",
                command.TransactionId,
                command.Amount);

            await PublishTransferFailed(command, "InvalidAmount", ct);
            return;
        }


        var fromAccVO = new AccountNumberVO(command.FromAccountNumber);

        var fromAccount = await _accountRepository.GetByAccountNumberAsync(fromAccVO, ct);

        if (fromAccount == null)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: source account {AccountNumber} not found",
                command.TransactionId,
                command.FromAccountNumber);

            await PublishTransferFailed(command, "FromAccountNotFound", ct);
            return;
        }


        if (command.InitiatorId != fromAccount.UserId)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: ownership mismatch for account {AccountNumber}",
                command.TransactionId,
                command.FromAccountNumber);

            await PublishTransferFailed(command, "OwnershipMismatch", ct);
            return;
        }


        var toAccVo = new AccountNumberVO(command.ToAccountNumber);

        var toAccount = await _accountRepository.GetByAccountNumberAsync(toAccVo, ct);

        if (toAccount == null)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: destination account {AccountNumber} not found",
                command.TransactionId,
                command.ToAccountNumber);

            await PublishTransferFailed(command, "ToAccountNotFound", ct);
            return;
        }


        var currency = (Currency)command.Currency;

        if (currency != fromAccount.Balance.Currency)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: source currency mismatch. Expected {ExpectedCurrency}, received {Currency}",
                command.TransactionId,
                fromAccount.Balance.Currency,
                command.Currency);

            await PublishTransferFailed(command, "SourceCurrencyMismatch", ct);
            return;
        }


        if (currency != toAccount.Balance.Currency)
        {
            _logger.LogWarning(
                "TransferMoneyCommand failed for transaction {TransactionId}: destination currency mismatch. Expected {ExpectedCurrency}, received {Currency}",
                command.TransactionId,
                toAccount.Balance.Currency,
                command.Currency);

            await PublishTransferFailed(command, "DestinationCurrencyMismatch", ct);
            return;
        }


        var moneyVO = new MoneyVO(command.Amount, currency);

        fromAccount.TransferTo(
            toAccount,
            moneyVO,
            command.TransactionId);


        _logger.LogInformation(
            "TransferMoneyCommand applied successfully for transaction {TransactionId}, from {FromAccountNumber} to {ToAccountNumber}, amount {Amount} {Currency}",
            command.TransactionId,
            command.FromAccountNumber,
            command.ToAccountNumber,
            command.Amount,
            command.Currency);
    }
}