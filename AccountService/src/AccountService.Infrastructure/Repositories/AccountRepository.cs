using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Data;
using AccountService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using AccountService.Domain.ValueObjects;
namespace AccountService.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{

    private readonly AccountDbContext _context;

    public AccountRepository (AccountDbContext context)
    {
        _context = context; 
    }


    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Accounts.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<Account?> GetByAccountNumberAsync(AccountNumberVO accountNumber, CancellationToken ct)
    {
        return await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == accountNumber, ct);
    }

    public async Task<bool> ExistsByAccountNumberAsync (AccountNumberVO accountNumber, CancellationToken ct)
    {
        return await _context.Accounts.AnyAsync(u => u.AccountNumber == accountNumber, ct);
    }

    public async Task AddAsync(Account account, CancellationToken ct)
    {
        await _context.Accounts.AddAsync(account, ct);
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync (CancellationToken ct)
    {
        return await _context.Accounts
            .AsNoTracking()
            .ToListAsync(ct);
    }
    public async Task DeleteAsync (Guid accId, CancellationToken ct)
    {
        var acc = await _context.Accounts.FindAsync(accId, ct);
        if (acc != null)
        {
            _context.Accounts.Remove(acc);
        }
    }


}
