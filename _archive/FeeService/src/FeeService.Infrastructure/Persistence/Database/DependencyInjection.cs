using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Persistence.Database;

public static class DependencyInjection
{

    public static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FeeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FeeDb")));

        services.AddHostedService<FeeDbContextMigrator>();
        return services;
    }

}
