using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        return services;
    }
}
