using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Consuming;
using TransactionService.Infrastructure.Messaging.Publishing;
using TransactionService.Infrastructure.Persistence.Inbox;
using TransactionService.Infrastructure.Persistence.Outbox;
using TransactionService.Infrastructure.Persistence.UnitOfWork;
using TransactionService.Infrastructure.Repositories;

namespace TransactionService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMessaging(configuration);

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddHostedService<InboxProcessor>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<AccountEventsConsumer>();  

        return services;
    }
}
