using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FeeService.Infrastructure.Persistence.Database;
using FeeService.Infrastructure.Persistence.Inbox;
using FeeService.Infrastructure.Messaging;

namespace FeeService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext(configuration);
        services.AddInbox();
        services.AddMessaging(configuration);


        return services;
    }
}
