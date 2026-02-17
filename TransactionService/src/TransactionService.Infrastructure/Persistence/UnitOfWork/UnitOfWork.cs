using TransactionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Exceptions;
using TransactionService.Application.Interfaces;
namespace TransactionService.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TransactionDbContext _context;

    public UnitOfWork(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("Concurrent update detected");
        }
    }
}
