using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Infrastructure.Messaging.Options;

namespace TransactionService.Infrastructure.Extensions;

public static class MessagingExtensions
{

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.Configure<TransactionEventsPublisherOptions>(
            configuration.GetSection("TransactionEventsPublisher"));

        services.Configure<AccountEventsConsumerOptions>(
            configuration.GetSection("AccountEventsConsumer"));

        return services;
    }

}
