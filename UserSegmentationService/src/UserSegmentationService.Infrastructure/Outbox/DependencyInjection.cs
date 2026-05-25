using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.Interfaces.Messaging;

namespace UserSegmentationService.Infrastructure.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox(
        this IServiceCollection services)
    {
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        return services;
    }

    public static IServiceCollection AddOutboxProcessing(
        this IServiceCollection services)
    {
        services.AddScoped<OutboxDispatcher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
