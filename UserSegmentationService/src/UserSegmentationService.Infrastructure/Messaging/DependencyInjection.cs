using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Infrastructure.Messaging.Consumers.Users;

namespace UserSegmentationService.Infrastructure.Messaging;

public static class DependencyInjection
{

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IInboxWriter, InboxWriter>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost");

                cfg.ReceiveEndpoint("segmentation.auth.events", e =>
                {
                    e.ConfigureConsumer<UserCreatedConsumer>(context);

                    e.Bind("auth.events", bind =>
                    {
                        bind.RoutingKey = "user.created";
                        bind.ExchangeType = "topic";
                    });
                });
            });
        });

        return services;
    }

}
