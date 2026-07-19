using AuthService.Application.Abstractions.Authentication;
using AuthService.Infrastructure.Authentication.Jwt;
using AuthService.Infrastructure.Authentication.RefreshTokens;
using AuthService.Infrastructure.Authentication.TokenBlacklisting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AuthService.Infrastructure.Authentication;

internal static class DependencyInjection
{
    internal static IServiceCollection AddAuthenticationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);

        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException(
                    "Redis connection string is not configured.");

            return ConnectionMultiplexer.Connect(connectionString);
        });

        services.AddSingleton<ITokenBlacklistStore, RedisTokenBlacklistStore>();

        return services;
    }
}
