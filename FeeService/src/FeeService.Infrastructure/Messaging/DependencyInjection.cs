using FeeService.Infrastructure.Messaging.Consuming.UserConsumers;
using FeeService.Infrastructure.Messaging.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitOptions = configuration
            .GetSection("RabbitMq")
            .Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMq options are not configured.");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserCreatedConsumer>();
            x.AddConsumer<UserDeletedConsumer>();
            x.AddConsumer<UserActivatedConsumer>();
            x.AddConsumer<UserDeactivatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host($"rabbitmq://{rabbitOptions.Host}:{rabbitOptions.Port}", h =>
                {
                    h.Username(rabbitOptions.Username);
                    h.Password(rabbitOptions.Password);
                });

                cfg.UseRawJsonDeserializer();

                cfg.ReceiveEndpoint("fee.auth.events", e =>
                {
                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                    e.ConfigureConsumer<UserDeletedConsumer>(context);
                    e.ConfigureConsumer<UserActivatedConsumer>(context);
                    e.ConfigureConsumer<UserDeactivatedConsumer>(context);

                    e.Bind("auth.events", s =>
                    {
                        s.RoutingKey = "user.*";
                        s.ExchangeType = "topic";
                    });
                });
            });
        });

        return services;
    }
}
