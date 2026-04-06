using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(EmailVO email, CancellationToken cancellationToken);
    Task CreateAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(User user, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(EmailVO email, CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);
}
