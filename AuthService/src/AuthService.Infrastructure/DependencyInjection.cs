using AuthService.Infrastructure.Authentication;
using AuthService.Infrastructure.EmailConfirmation;
using AuthService.Infrastructure.Grpc;
using AuthService.Infrastructure.Messaging;
using AuthService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddAuthenticationInfrastructure(configuration);
        services.AddEmailConfirmation(configuration);
        services.AddMessaging(configuration);
        services.AddGrpcClients(configuration);

        return services;
    }
}
