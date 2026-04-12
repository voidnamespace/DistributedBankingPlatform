using FeeService.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Extensions;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FeeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FeeDb")));

        return services;
    }
}
