using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Application.IntegrationEvents.Accounts;
using UserSegmentationService.Infrastructure.Inbox;
using UserSegmentationService.Infrastructure.Messaging;
using UserSegmentationService.Infrastructure.Persistence;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddScoped<IUserMetricRepository, UserMetricRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<ISegmentRepository, SegmentRepository>();
        services.AddScoped<ISegmentMembershipRepository, SegmentMembershipRepository>();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(
                typeof(AccountCreatedIntegrationEvent).Assembly));
        services.AddInbox();
        services.AddMessaging(configuration);

        return services;
    }
}
