using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Infrastructure.Messaging.Consumers.Accounts;
using UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

namespace UserSegmentationService.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserCreatedConsumer>();
            x.AddConsumer<UserActivatedConsumer>();
            x.AddConsumer<UserDeactivatedConsumer>();
            x.AddConsumer<UserDeletedConsumer>();
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

                cfg.ReceiveEndpoint("segmentation.auth.events", e =>
                {
                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                    e.ConfigureConsumer<UserActivatedConsumer>(context);
                    e.ConfigureConsumer<UserDeactivatedConsumer>(context);
                    e.ConfigureConsumer<UserDeletedConsumer>(context);

                    e.Bind("auth.events", bind =>
                    {
                        bind.RoutingKey = "user.created";
                        bind.ExchangeType = "topic";
                    });

                    e.Bind("auth.events", bind =>
                    {
                        bind.RoutingKey = "user.activated";
                        bind.ExchangeType = "topic";
                    });

                    e.Bind("auth.events", bind =>
                    {
                        bind.RoutingKey = "user.deactivated";
                        bind.ExchangeType = "topic";
                    });

                    e.Bind("auth.events", bind =>
                    {
                        bind.RoutingKey = "user.deleted";
                        bind.ExchangeType = "topic";
                    });
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
