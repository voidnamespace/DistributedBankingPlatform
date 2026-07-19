using AuthService.Domain.Entities;
using System.Security.Claims;

namespace AuthService.Application.Abstractions.Authentication;

public interface IJwtService
{
    AccessTokenResult GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}

public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);
