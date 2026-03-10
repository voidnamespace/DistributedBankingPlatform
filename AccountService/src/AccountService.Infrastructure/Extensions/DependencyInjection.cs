using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Messaging.Consuming;
using AccountService.Infrastructure.Messaging.Options;
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
        services.Configure<AuthEventsConsumerOptions>(
    configuration.GetSection("RabbitMQ"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<AuthEventsConsumer>();
        services.AddHostedService<InboxProcessor>();
        services.AddScoped<IInboxWriter, InboxWriter>();


        return services;
    }
}
