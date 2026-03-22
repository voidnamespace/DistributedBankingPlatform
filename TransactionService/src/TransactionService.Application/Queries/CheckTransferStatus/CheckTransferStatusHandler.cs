using MediatR;
using TransactionService.Domain.Enums;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Queries.CheckTransferStatus;

public class CheckTransferStatusHandler : IRequestHandler<CheckTransferStatusQuery, TransactionStatus>
{
    private readonly ITransactionRepository _repository;

    public CheckTransferStatusHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionStatus> Handle (CheckTransferStatusQuery query, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(query.TransactionId, ct);

        if (transaction == null)
            throw new KeyNotFoundException($"Transaction with id {query.TransactionId} not found");

        return transaction.Status;
    }

}
