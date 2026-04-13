using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Messaging.Inbox;
using FeeService.Infrastructure.Messaging.Inbox.Handlers.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Persistence.Inbox;

public static class DependencyInjection
{
    public static IServiceCollection AddInbox(
    this IServiceCollection services)
    {
        services.AddScoped<IInboxWriter, InboxWriter>();

        services.AddScoped<IInboxDispatcher, InboxDispatcher>();

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

        services.AddHostedService<InboxProcessor>();

        return services;
    }
}
