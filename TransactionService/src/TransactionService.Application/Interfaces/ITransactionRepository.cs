using TransactionService.Domain.Entities;
namespace TransactionService.Application.Interfaces;
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct);

}
