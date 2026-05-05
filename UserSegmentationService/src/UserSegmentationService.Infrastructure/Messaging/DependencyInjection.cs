using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Infrastructure.Messaging.Consumers.Accounts;

namespace UserSegmentationService.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AccountCreatedConsumer>();
            x.AddConsumer<TransferSuccessConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint("segmentation.account.events", e =>
                {
                    e.ConfigureConsumer<AccountCreatedConsumer>(context);
                    e.ConfigureConsumer<TransferSuccessConsumer>(context);

                    e.Bind("account.events", bind =>
                    {
                        bind.RoutingKey = "account.created";
                        bind.ExchangeType = "topic";
                    });

                    e.Bind("account.events", bind =>
                    {
                        bind.RoutingKey = "transfer.success";
                        bind.ExchangeType = "topic";
                    });
                });
            });
        });

        return services;
    }
}
