using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkTransactionSuccess;

public class MarkTransactionSuccessHandler : IRequestHandler<MarkTransactionSuccessCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkTransactionSuccessHandler> _logger;

    public MarkTransactionSuccessHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkTransactionSuccessHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkTransactionSuccessCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkTransactionSuccessCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkTransactionSuccessCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Complete();

        _logger.LogInformation(
            "Transaction marked as success: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Transaction success persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
