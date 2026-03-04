using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Inbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure.Extensions;

public static class DI
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);

        services.AddRepositories();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<AccountEventsConsumer>();
        services.AddHostedService<InboxProcessor>();


        return services;
    }
}
