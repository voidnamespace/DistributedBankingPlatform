using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Data;
namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;

    public TransactionRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        await _context.Transactions.AddAsync(transaction, ct);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

}
