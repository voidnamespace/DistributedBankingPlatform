namespace AnalyticsService.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AnalyticsOptions>(
            configuration.GetSection(AnalyticsOptions.SectionName));

        return services;
    }
}
