using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public class CreateWithdrawalHandler
    : IRequestHandler<CreateWithdrawalCommand, Guid>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateWithdrawalHandler> _logger;

    public CreateWithdrawalHandler(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateWithdrawalHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateWithdrawalCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateWithdrawalCommand started: initiator {InitiatorId}, from {FromAccountNumber}, amount {Amount}, currency {Currency}",
            cmd.InitiatorId,
            cmd.FromAccountNumber,
            cmd.Amount,
            cmd.Currency);

        Currency currency = (Currency)cmd.Currency;

        var money = new MoneyVO(cmd.Amount, currency);

        var fromAccountNumber = new AccountNumberVO(cmd.FromAccountNumber);

        var transaction = Transaction.CreateWithdrawal(
            cmd.InitiatorId,
            fromAccountNumber,
            money);

        _logger.LogInformation(
            "Withdrawal transaction initialized: initiator {InitiatorId}, from {FromAccountNumber}, amount {Amount}, currency {Currency}",
            cmd.InitiatorId,
            cmd.FromAccountNumber,
            cmd.Amount,
            cmd.Currency);

        await _transactionRepository.AddAsync(transaction, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Withdrawal transaction completed: TransactionId {TransactionId}",
            transaction.TransactionId);

        return transaction.TransactionId;
    }
}
