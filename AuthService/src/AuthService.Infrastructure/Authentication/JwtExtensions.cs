using System.Security.Claims;
using System.Text;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Authentication;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuthentication");

                        var userRepo = ctx.HttpContext.RequestServices
                            .GetRequiredService<IUserRepository>();

                        var userIdStr = ctx.Principal?
                            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (!Guid.TryParse(userIdStr, out var userId))
                        {
                            logger.LogWarning("JWT token validation failed: invalid user id claim");

                            ctx.Fail("Invalid token.");
                            return;
                        }

                        var user = await userRepo.GetByIdAsync(
                            userId,
                            ctx.HttpContext.RequestAborted);

                        if (user is null)
                        {
                            logger.LogWarning("JWT token rejected: user {UserId} not found", userId);
                            ctx.Fail("User not found.");
                            return;
                        }

                        if (!user.IsActive)
                        {
                            logger.LogWarning("JWT token rejected: user {UserId} is inactive", userId);
                            ctx.Fail("User inactive.");
                            return;
                        }

                        logger.LogInformation("JWT token validated for user {UserId}", userId);
                    }
                };
            });

        return services;
    }
}
