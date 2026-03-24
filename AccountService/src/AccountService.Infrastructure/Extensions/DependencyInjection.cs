using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Messaging.Consuming;
using AccountService.Infrastructure.Messaging.Options;
using AccountService.Infrastructure.Messaging.Publishing;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Inbox;
using AccountService.Infrastructure.Persistence.Outbox;
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
            configuration.GetSection("AuthEvents"));

        services.Configure<TransactionEventsConsumerOptions>(
            configuration.GetSection("TransactionEvents"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<AuthEventsConsumer>();
        services.AddHostedService<TransactionEventsConsumer>();
        services.AddHostedService<InboxProcessor>();
        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddHostedService<OutboxProcessor>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));
        services.Configure<AccountEventsPublisherOptions>(
    configuration.GetSection("AccountEventsPublisher"));

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
