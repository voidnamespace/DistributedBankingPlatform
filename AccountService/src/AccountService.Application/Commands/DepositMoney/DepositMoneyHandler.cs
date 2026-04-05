using AccountService.Application.IntegrationEvents.Transactions.Deposit;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.DepositMoney;

public class DepositMoneyHandler : IRequestHandler<DepositMoneyCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DepositMoneyHandler> _logger;
    private readonly IOutboxWriter _outboxWriter;

    public DepositMoneyHandler(
        IAccountRepository accountRepository,
        ILogger<DepositMoneyHandler> logger,
        IOutboxWriter outboxWriter)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _outboxWriter = outboxWriter;
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

        if (command.Amount <= 0)
        {
            await _outboxWriter.EnqueueAsync(new DepositFailedIntegrationEvent(
                command.TransactionId,
                command.ToAccountNumber,
                command.Amount,
                command.Currency), ct);

            return;
        }

        if ((Currency)command.Currency != acc.Balance.Currency)
        {
            await _outboxWriter.EnqueueAsync(new DepositFailedIntegrationEvent(
                command.TransactionId,
                command.ToAccountNumber,
                command.Amount,
                command.Currency), ct);

            return;
        }

        var money = new MoneyVO(
            command.Amount,
            (Currency)command.Currency
        );

        acc.Deposit(money, command.TransactionId);

        _logger.LogInformation(
            "Deposit completed for account {AccountNumber}, amount {Amount} {Currency}",
            command.ToAccountNumber,
            command.Amount,
            command.Currency);
    }
}