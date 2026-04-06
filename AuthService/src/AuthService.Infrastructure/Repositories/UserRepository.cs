using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(EmailVO email, CancellationToken ct)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        user.Touch();
        _context.Users.Update(user);
    }

    public Task DeleteAsync(User user, CancellationToken ct)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByEmailAsync(
        EmailVO email,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .ToListAsync();
    }
}
