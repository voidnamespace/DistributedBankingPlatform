using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Inbox.Handlers.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Inbox;

public static class DependencyInjection
{
    public static IServiceCollection AddInbox(this IServiceCollection services)
    {
        services.AddScoped<IInboxWriter, InboxWriter>();
        services.AddScoped<IInboxDispatcher, InboxDispatcher>();
        services.AddHostedService<InboxProcessor>();

        services.AddScoped<
            IInboxMessageHandler<UserCreatedIntegrationEvent>,
            UserCreatedInboxHandler>();

        services.AddScoped<
            IInboxMessageHandler<UserDeletedIntegrationEvent>,
            UserDeletedInboxHandler>();

        services.AddScoped<
            IInboxMessageHandler<UserActivatedIntegrationEvent>,
            UserActivatedInboxHandler>();

        services.AddScoped<
            IInboxMessageHandler<UserDeactivatedIntegrationEvent>,
            UserDeactivatedInboxHandler>();

        return services;
    }
}
