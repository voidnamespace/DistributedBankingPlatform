using FeeService.Infrastructure.Data;
using FeeService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FeeService.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure(Configuration);

        services.AddControllers();
    }
    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FeeDbContext>();

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
