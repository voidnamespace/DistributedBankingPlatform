using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;
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

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var allUsers = await _context.Users
            .Include(u => u.RefreshTokens)
            .ToListAsync();

        return allUsers.FirstOrDefault(u =>
            u.Email.Value.ToLower() == email.ToLower());
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return;

        user.Deactivate();
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _context.Users
            .AnyAsync(
                u => u.Email.Value.ToLower() == normalizedEmail,
                cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .ToListAsync();
    }
}