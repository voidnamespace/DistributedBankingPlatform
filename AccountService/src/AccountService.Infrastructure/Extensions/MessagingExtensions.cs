using AccountService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthEventsConsumerOptions>(
            configuration.GetSection("AuthEventsConsumer"));

        services.Configure<TransactionEventsConsumerOptions>(
            configuration.GetSection("TransactionEventsConsumer"));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.Configure<AccountEventsPublisherOptions>(
            configuration.GetSection("AccountEventsPublisher"));

        return services;
    }
}
