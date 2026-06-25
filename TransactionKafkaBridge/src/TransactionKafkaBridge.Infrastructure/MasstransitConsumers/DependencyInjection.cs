using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionKafkaBridge.Infrastructure.Kafka;
using TransactionKafkaBridge.Infrastructure.MasstransitConsumers.Transfer;

namespace TransactionKafkaBridge.Infrastructure.MasstransitConsumers;

public static class DependencyInjection
{
    public static IServiceCollection AddConsuming(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(
            configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<TransferKafkaProducer>();

        services.AddMassTransit(x =>
        {
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

                cfg.UseRawJsonDeserializer();

                cfg.ReceiveEndpoint("bridge.account.transfersuccess", e =>
                {
                    e.ConfigureConsumer<TransferSuccessConsumer>(context);

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
