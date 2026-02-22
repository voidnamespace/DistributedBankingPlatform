using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Authentication;
using AuthService.Infrastructure.Caching;
using AuthService.Infrastructure.Messaging.Options;
using AuthService.Infrastructure.Messaging.Publishing;
using AuthService.Infrastructure.Persistence.Seeding;
using AuthService.Infrastructure.Persistence.UnitOfWork;
using AuthService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
namespace AuthService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddDatabaseConfiguration(configuration);
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<AuthDbSeeder>();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString =
                configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis connection string not configured");

            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddSingleton<IRedisService, RedisService>();
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
