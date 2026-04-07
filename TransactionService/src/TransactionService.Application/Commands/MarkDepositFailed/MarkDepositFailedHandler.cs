using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkDepositFailed;

public class MarkDepositFailedHandler : IRequestHandler<MarkDepositFailedCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkDepositFailedHandler> _logger;

    public MarkDepositFailedHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkDepositFailedHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkDepositFailedCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkDepositFailedCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkDepositFailedCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Fail();

        _logger.LogInformation(
            "Deposit transaction marked as failed: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deposit transaction failure persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
