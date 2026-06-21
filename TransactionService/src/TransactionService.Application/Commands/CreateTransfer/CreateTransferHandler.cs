using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.FraudDetection;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Application.Commands.CreateTransfer;

public class CreateTransferHandler
    : IRequestHandler<CreateTransferCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFraudDetectionClient _fraudDetectionClient;
    private readonly ILogger<CreateTransferHandler> _logger;

    public CreateTransferHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        IFraudDetectionClient fraudDetectionClient,
        ILogger<CreateTransferHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fraudDetectionClient = fraudDetectionClient;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateTransferCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateTransferCommand started: initiator {InitiatorId}, from {FromAccountNumber}, to {ToAccountNumber}, amount {Amount}, currency {Currency}",
            cmd.InitiatorId,
            cmd.FromAccountNumber,
            cmd.ToAccountNumber,
            cmd.Amount,
            cmd.Currency);
        
        Currency currency = (Currency)cmd.Currency;
        var money = new MoneyVO(cmd.Amount, currency);
        var fromAccountNumber = new AccountNumberVO(cmd.FromAccountNumber);
        var toAccountNumber = new AccountNumberVO(cmd.ToAccountNumber);

        var transaction = Transaction.CreateTransfer(
            cmd.InitiatorId,
            fromAccountNumber,
            toAccountNumber,
            money);

        var fraudCheck = await _fraudDetectionClient.CheckTransferAsync(
            new FraudCheckRequest(
                transaction.TransactionId,
                cmd.InitiatorId,
                cmd.FromAccountNumber,
                cmd.ToAccountNumber,
                cmd.Amount,
                currency.ToString()),
            ct);

        _logger.LogInformation(
            "Fraud check completed: transaction {TransactionId}, decision {Decision}, risk score {RiskScore}, reasons {Reasons}",
            transaction.TransactionId,
            fraudCheck.Decision,
            fraudCheck.RiskScore,
            string.Join("; ", fraudCheck.Reasons));

        if (fraudCheck.Decision == FraudCheckDecision.Rejected)
        {
            throw new DomainException(
                $"Transfer rejected by fraud detection. Risk score: {fraudCheck.RiskScore}. Reasons: {string.Join("; ", fraudCheck.Reasons)}");
        }

        _logger.LogInformation(
            "Transaction entity created: initiator {InitiatorId}, from {FromAccountNumber}, to {ToAccountNumber}, amount {Amount}, currency {Currency}",
            cmd.InitiatorId,
            cmd.FromAccountNumber,
            cmd.ToAccountNumber,
            cmd.Amount,
            cmd.Currency);

        await _repository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "CreateTransferCommand completed: TrasnsactionId {TransactionId},initiator {InitiatorId}, from {FromAccountNumber}, to {ToAccountNumber}, amount {Amount}, currency {Currency}",
            transaction.TransactionId,
            cmd.InitiatorId,
            cmd.FromAccountNumber,
            cmd.ToAccountNumber,
            cmd.Amount,
            cmd.Currency);

        return transaction.TransactionId;
    }
}
