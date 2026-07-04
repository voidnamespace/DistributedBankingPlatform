using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Data;
using AccountService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using AccountService.Domain.ValueObjects;
using Npgsql;
using NpgsqlTypes;

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
    public Task DeleteAsync(Account account, CancellationToken ct)
    {
        _context.Accounts.Remove(account);
        return Task.CompletedTask;
    }
    public async Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Account>> GetBackfillBatchAsync(
        int skip,
        int take,
        CancellationToken ct)
    {
        return await _context.Accounts
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<Account?> GetByAccountNumberForUpdateAsync(
    AccountNumberVO accountNumber,
    CancellationToken ct)
    {
        return await _context.Accounts
            .FromSqlInterpolated($"""
            SELECT * 
            FROM "Accounts" 
            WHERE "AccountNumber" = {accountNumber.Value} 
            FOR UPDATE
            """)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Account>> GetByAccountNumbersForUpdateAsync(
        IReadOnlyCollection<AccountNumberVO> accountNumbers,
        CancellationToken ct)
    {
        var values = accountNumbers
            .Select(x => x.Value)
            .Distinct()
            .ToArray();

        if (values.Length == 0)
        {
            return Array.Empty<Account>();
        }

        var accountNumbersParameter = new NpgsqlParameter<string[]>("accountNumbers", values)
        {
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text
        };

        return await _context.Accounts
            .FromSqlRaw("""
            SELECT * 
            FROM "Accounts" 
            WHERE "AccountNumber" = ANY(@accountNumbers) 
            ORDER BY "AccountNumber" 
            FOR UPDATE
            """, accountNumbersParameter)
            .ToListAsync(ct);
    }
}
