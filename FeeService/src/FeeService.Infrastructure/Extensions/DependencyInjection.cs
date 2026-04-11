using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Persistence.Inbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);

        services.AddMessaging(configuration);

        services.AddScoped<IInboxWriter, InboxWriter>();

        services.AddScoped<IInboxMessageHandler, InboxMessageHandler>();

        services.AddHostedService<InboxProcessor>();

        return services;
    }
}
