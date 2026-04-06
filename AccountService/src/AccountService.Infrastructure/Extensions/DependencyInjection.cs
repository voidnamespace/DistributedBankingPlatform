using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Messaging.Consuming;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMessaging(configuration);

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        services.AddHostedService<AuthEventsConsumer>();
        services.AddHostedService<TransactionEventsConsumer>();

        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddHostedService<InboxProcessor>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
