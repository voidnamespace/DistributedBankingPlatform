using FeeService.Infrastructure.Messaging.Consuming;
using FeeService.Infrastructure.Messaging.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitOptions = configuration
                    .GetSection("RabbitMq")
                    .Get<RabbitMqOptions>();

                cfg.Host(rabbitOptions!.Host, h =>
                {
                    h.Username(rabbitOptions.Username);
                    h.Password(rabbitOptions.Password);
                });

                cfg.UseRawJsonDeserializer();

                cfg.ReceiveEndpoint("fee-service-user-events", e =>
                {
                    e.ConfigureConsumer<UserCreatedConsumer>(context);

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
