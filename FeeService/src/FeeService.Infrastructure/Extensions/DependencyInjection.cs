using FeeService.Infrastructure.Messaging.Consumers;
using FeeService.Infrastructure.Inbox;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
