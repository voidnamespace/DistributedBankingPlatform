using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkTransactionFailed;

public class MarkTransactionFailedHandler : IRequestHandler<MarkTransactionFailedCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkTransactionFailedHandler> _logger;

    public MarkTransactionFailedHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkTransactionFailedHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkTransactionFailedCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkTransactionFailedCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkTransactionFailedCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Fail();

        _logger.LogInformation(
            "Transaction marked as failed: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Transaction failure persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
