using AccountService.API.Extensions;
using AccountService.Application;
using AccountService.Infrastructure.Data;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using AccountService.Infrastructure.Extensions;
namespace AccountService.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
        services.AddInfrastructure(Configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();
        services.AddHealthChecksConfiguration(Configuration);
        services.AddRateLimitingConfiguration(Configuration);

    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseSwaggerConfiguration();
        app.UseIpRateLimiting();
        app.MapControllers();
        app.MapHealthCheckEndpoints();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("MIGRATIONS APPLIED");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WAITING FOR DB... " + ex.Message);
                    Thread.Sleep(3000);
                    retries--;
                }
            }
        }
    }
}
