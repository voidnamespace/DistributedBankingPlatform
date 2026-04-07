using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkWithdrawalFailed;

public class MarkWithdrawalFailedHandler : IRequestHandler<MarkWithdrawalFailedCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkWithdrawalFailedHandler> _logger;

    public MarkWithdrawalFailedHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkWithdrawalFailedHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkWithdrawalFailedCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkWithdrawalFailedCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkWithdrawalFailedCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Fail();

        _logger.LogInformation(
            "Withdrawal transaction marked as failed: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Withdrawal transaction failure persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
