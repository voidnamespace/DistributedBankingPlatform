using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Queries.CheckTransferStatus;

public class CheckTransferStatusHandler
    : IRequestHandler<CheckTransferStatusQuery, string>
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<CheckTransferStatusHandler> _logger;

    public CheckTransferStatusHandler(
        ITransactionRepository repository,
        ILogger<CheckTransferStatusHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> Handle(
        CheckTransferStatusQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "CheckTransferStatusQuery received: TransactionId {TransactionId}",
            query.TransactionId);

        var transaction = await _repository.GetByIdAsync(
            query.TransactionId,
            ct);

        if (transaction == null)
        {
            _logger.LogWarning(
                "CheckTransferStatusQuery failed: transaction not found {TransactionId}",
                query.TransactionId);

            throw new KeyNotFoundException(
                $"Transaction with id {query.TransactionId} not found");
        }

        _logger.LogInformation(
            "CheckTransferStatusQuery succeeded: TransactionId {TransactionId}, Status {Status}",
            query.TransactionId,
            transaction.Status);

        return transaction.Status.ToString();
    }
}
