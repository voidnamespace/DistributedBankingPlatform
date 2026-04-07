using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Application.Commands.CreateDeposit;

public class CreateDepositHandler : IRequestHandler<CreateDepositCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDepositHandler> _logger;

    public CreateDepositHandler(
        ITransactionRepository repository, 
        IUnitOfWork unitOfWork,
        ILogger<CreateDepositHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateDepositCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation(
             "CreateDepositCommand started for {ToAccountNumber}, amount {Amount} {Currency}",
             cmd.ToAccountNumber,
             cmd.Amount,
             cmd.Currency);

        Currency currency = (Currency)cmd.Currency;

        var money = new MoneyVO(cmd.Amount, currency);

        var toAccountNumber = new AccountNumberVO(cmd.ToAccountNumber);

        var transaction = Transaction.CreateDeposit(
            toAccountNumber,
            money);

        _logger.LogInformation(
            "Transaction entity created for {ToAccountNumber} with Id {TransactionId}",
            cmd.ToAccountNumber,
            transaction.TransactionId);

        await _repository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "CreateDepositCommand completed for {ToAccountNumber}, {transactionId}",
            cmd.ToAccountNumber,
            transaction.TransactionId);

        return transaction.TransactionId;

    }

}
