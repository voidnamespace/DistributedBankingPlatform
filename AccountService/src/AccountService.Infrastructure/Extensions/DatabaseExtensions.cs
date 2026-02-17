using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccountService.Infrastructure.Data;
namespace AccountService.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("AccountDb")
            ?? throw new InvalidOperationException(
                "Connection string 'AccountDb' not found");

        services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(cs, b =>
        b.MigrationsAssembly(typeof(AccountDbContext).Assembly.FullName)));


        return services;
    }
}
