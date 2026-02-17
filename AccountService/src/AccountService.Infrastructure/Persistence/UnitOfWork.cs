using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AccountService.Application.Exceptions;
namespace AccountService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;

    public UnitOfWork(AccountDbContext context)
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
