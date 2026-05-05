using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Inbox;

public static class DependencyInjection
{
    public static IServiceCollection AddInbox(
        this IServiceCollection services)
    {
        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddScoped<InboxDispatcher>();
        services.AddHostedService<InboxProcessor>();

        return services;
    }
}
