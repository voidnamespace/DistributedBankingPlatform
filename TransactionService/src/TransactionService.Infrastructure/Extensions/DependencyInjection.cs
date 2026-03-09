
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Persistence.Outbox;
using TransactionService.Infrastructure.Persistence.UnitOfWork;

namespace TransactionService.Infrastructure.Extensions;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration);
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddHostedService<OutboxProcessor>();


    }



}
