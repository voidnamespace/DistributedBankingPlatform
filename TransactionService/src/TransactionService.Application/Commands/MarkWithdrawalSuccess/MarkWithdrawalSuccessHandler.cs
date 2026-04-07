using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkWithdrawalSuccess;

public class MarkWithdrawalSuccessHandler : IRequestHandler<MarkWithdrawalSuccessCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkWithdrawalSuccessHandler> _logger;

    public MarkWithdrawalSuccessHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkWithdrawalSuccessHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkWithdrawalSuccessCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkWithdrawalSuccessCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkWithdrawalSuccessCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Complete();

        _logger.LogInformation(
            "Withdrawal transaction marked as success: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Withdrawal transaction success persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
