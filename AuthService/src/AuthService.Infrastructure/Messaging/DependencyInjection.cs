using AuthService.Application.Abstractions.Messaging;
using AuthService.Infrastructure.Messaging.Outbox;
using AuthService.Infrastructure.Messaging.RabbitMq.Options;
using AuthService.Infrastructure.Messaging.RabbitMq.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Messaging;

internal static class DependencyInjection
{
    internal static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.Configure<AuthEventsPublisherOptions>(
            configuration.GetSection("AuthEventsPublisher"));

        services.AddSingleton<IMessagePublisher, RabbitMqEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
