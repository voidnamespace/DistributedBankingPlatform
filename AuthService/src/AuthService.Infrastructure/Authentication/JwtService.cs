using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
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
        if (user == null)
        {
            _logger.LogWarning("GenerateAccessToken failed: user is null");
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogCritical("GenerateAccessToken failed: secret key missing");
            throw new InvalidOperationException("Secret key not configured");
        }

        _logger.LogInformation("GenerateAccessToken started {UserId}", user.Id);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        _logger.LogDebug(
            "Generating claims for user {UserId} with role {Role}",
            user.Id,
            user.Role);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email._email),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
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

        _logger.LogDebug(
            "Access token generated for {UserId} expires at {Expiration}",
            user.Id,
            tokenDescriptor.Expires);

        _logger.LogInformation("GenerateAccessToken complete {UserId}", user.Id);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        _logger.LogDebug("GenerateRefreshToken started");

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        _logger.LogDebug("GenerateRefreshToken completed");

        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        _logger.LogDebug("ValidateToken started");

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogCritical("ValidateToken failed: secret key missing");
            throw new InvalidOperationException("Secret key not configured");
        }

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

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning(
                    "JWT token validation failed: invalid algorithm");

                return null;
            }

            var userId =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation(
                "JWT token validated successfully for {UserId}",
                userId);

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("JWT token validation failed: token expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("JWT token validation failed: invalid signature");
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
        _logger.LogDebug("GetUserIdFromToken started");

        var principal = ValidateToken(token);

        if (principal == null)
        {
            _logger.LogWarning(
                "GetUserIdFromToken failed: token validation failed");

            return null;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            _logger.LogWarning(
                "GetUserIdFromToken failed: NameIdentifier claim missing");

            return null;
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning(
                "GetUserIdFromToken failed: invalid Guid format");

            return null;
        }

        _logger.LogDebug(
            "UserId extracted successfully from JWT: {UserId}",
            userId);

        return userId;
    }
}
