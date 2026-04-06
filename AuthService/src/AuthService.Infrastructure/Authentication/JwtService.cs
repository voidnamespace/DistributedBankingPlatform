using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AuthService.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(
        IConfiguration configuration,
        ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT Secret Key not found in configuration");

        _issuer = _configuration["Jwt:Issuer"] ?? "AuthService";
        _audience = _configuration["Jwt:Audience"] ?? "AuthServiceClient";
    }

    public string GenerateAccessToken(User user)
    {
        _logger.LogInformation("Generating access token for user {UserId}", user.Id);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email._email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        _logger.LogInformation("Access token generated for user {UserId}", user.Id);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        _logger.LogDebug("Generating refresh token");

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        _logger.LogDebug("Validating JWT token");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation("JWT token validated successfully");
                return principal;
            }

            _logger.LogWarning("JWT token validation failed: invalid algorithm");

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return null;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);

        if (principal == null)
        {
            _logger.LogWarning("Failed to extract user id from token");
            return null;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("UserId claim missing or invalid in JWT");
            return null;
        }

        return userId;
    }
}
