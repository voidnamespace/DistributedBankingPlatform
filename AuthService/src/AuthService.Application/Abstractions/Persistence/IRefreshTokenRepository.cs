using AuthService.Domain.Entities;

namespace AuthService.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken);

    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken);
}
