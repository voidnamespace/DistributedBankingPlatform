using EventProjectionService.Infrastructure.Messaging.Consuming;
using EventProjectionService.Infrastructure.Messaging.Options;
using EventProjectionService.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventProjectionService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoOptions>(
            configuration.GetSection("Mongo"));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMQ"));
        services.Configure<AuthEventsConsumerOptions>(
            configuration.GetSection("AuthEvents"));

        services.Configure<ProjectionEventsConsumerOptions>(
            configuration.GetSection("ProjectionEvents"));

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<MongoEventRepository>();

        services.AddHostedService<ProjectionEventsConsumer>();

        return services;
    }
}