using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkDepositSuccess;

public class MarkDepositSuccessHandler : IRequestHandler<MarkDepositSuccessCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkDepositSuccessHandler> _logger;

    public MarkDepositSuccessHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkDepositSuccessHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        MarkDepositSuccessCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MarkDepositSuccessCommand started: TransactionId {TransactionId}",
            command.TransactionId);

        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "MarkDepositSuccessCommand failed: transaction not found {TransactionId}",
                command.TransactionId);

            throw new KeyNotFoundException("Transaction not found");
        }

        transaction.Complete();

        _logger.LogInformation(
            "Deposit transaction marked as success: TransactionId {TransactionId}",
            command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deposit transaction success persisted: TransactionId {TransactionId}",
            command.TransactionId);
    }
}
