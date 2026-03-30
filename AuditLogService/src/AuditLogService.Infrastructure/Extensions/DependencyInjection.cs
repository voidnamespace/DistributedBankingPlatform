using AuditLogService.Infrastructure.Messaging.Consuming;
using AuditLogService.Infrastructure.Messaging.Options;
using AuditLogService.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditLogService.Infrastructure.Extensions;

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
        services.Configure<AccountEventsConsumerOptions>(
            configuration.GetSection("AccountEvents"));
        services.Configure<TransactionEventsConsumerOptions>(
            configuration.GetSection("TransactionEvents"));
        services.Configure<AuditLogEventsConsumerOptions>(
            configuration.GetSection("AuditLogEvents"));

        services.AddScoped<MongoDbContext>();

        services.AddScoped<MongoEventRepository>();

        services.AddHostedService<AuditLogEventsConsumer>();

        return services;
    }
}
