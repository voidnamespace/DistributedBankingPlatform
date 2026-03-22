
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Consuming;
using TransactionService.Infrastructure.Messaging.Options;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        services.AddHostedService<AccountEventsConsumer>();
        services.Configure<AccountEventsConsumerOptions>(
            configuration.GetSection("account.events"));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));
        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddHostedService<InboxProcessor>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

}
