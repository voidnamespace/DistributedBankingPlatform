using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TransactionService.Infrastructure.Extensions;

public static class DatabaseServiceCollectionExtensions
{

    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("TransactionDb")
            ));

        return services;
    }

}
