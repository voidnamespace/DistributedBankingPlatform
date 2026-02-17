using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
namespace AccountService.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Account?> GetByAccountNumberAsync(AccountNumberVO accountNumber, CancellationToken ct);

    Task<bool> ExistsByAccountNumberAsync(AccountNumberVO accountNumber, CancellationToken ct);

    Task AddAsync(Account account, CancellationToken ct);

    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct);

    Task DeleteAsync(Guid accId, CancellationToken ct);

}
