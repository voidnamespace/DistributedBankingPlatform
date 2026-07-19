using AuthService.Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Authentication.RefreshTokens;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be null, empty, or whitespace.", nameof(refreshToken));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToBase64String(hash);
    }
}
