using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.IntegrationEvents.Accounts;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Infrastructure.BackgroundJobs;
using UserSegmentationService.Infrastructure.Caching;
using UserSegmentationService.Infrastructure.Inbox;
using UserSegmentationService.Infrastructure.Messaging;
using UserSegmentationService.Infrastructure.Persistence;
using UserSegmentationService.Infrastructure.Persistence.Database;
using UserSegmentationService.Infrastructure.Persistence.Repositories;
using UserSegmentationService.Infrastructure.Persistence.Seeding;

namespace UserSegmentationService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserMetricRepository, UserMetricRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<ISegmentRepository, SegmentRepository>();
        
        services.AddScoped<ISegmentMembershipRepository, SegmentMembershipRepository>();
        services.AddScoped<ISegmentDeltaRepository, SegmentDeltaRepository>();
        
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(
                typeof(AccountCreatedIntegrationEvent).Assembly));

        services.AddCaching(configuration);
        
        services.AddInbox();

        if (bool.TryParse(configuration["SegmentEvaluationBackgroundService:Enabled"], out var segmentEvaluationEnabled) && segmentEvaluationEnabled)
        {
            services.AddBackgroundJobs(configuration);
        }

        if (bool.TryParse(configuration["Messaging:Enabled"], out var messagingEnabled) && messagingEnabled)
        {
            services.AddMessaging(configuration);
        }

        return services;
    }
}
