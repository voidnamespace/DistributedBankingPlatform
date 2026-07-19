using AuthService.Application.Abstractions.ExternalServices.Accounts;
using AuthService.Infrastructure.Grpc.AccountService;
using AuthService.Infrastructure.Grpc.AccountService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Grpc;

internal static class DependencyInjection
{
    internal static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserDeletionValidator, GrpcUserDeletionValidator>();

        services.AddGrpcClient<UserLifecycleValidation.UserLifecycleValidationClient>(options =>
        {
            var address = configuration["UserLifecycleGrpc:Address"]
                ?? throw new InvalidOperationException(
                    "UserLifecycleGrpc:Address is not configured.");

            options.Address = new Uri(address);
        });

        return services;
    }
}
