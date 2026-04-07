using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        return refreshToken;
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens.FindAsync(id);
        if (token != null)
        {
            _context.RefreshTokens.Remove(token);
        }
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

    }

    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
    }
}
