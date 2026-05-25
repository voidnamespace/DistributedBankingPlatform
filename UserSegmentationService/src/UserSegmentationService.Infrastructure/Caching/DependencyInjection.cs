using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserSegmentationService.Application.Interfaces;

namespace UserSegmentationService.Infrastructure.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISegmentCache, SegmentCache>();
        services.AddMemoryCache();

        return services;
    }

}
