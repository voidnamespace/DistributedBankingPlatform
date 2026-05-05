using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace UserSegmentationService.Infrastructure.Persistence.Database;

public static class DependencyInjection
{

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SegmentationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("SegmentationDb")));

        return services;
    }

}
