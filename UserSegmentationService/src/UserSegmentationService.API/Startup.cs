using UserSegmentationService.Infrastructure.Extensions;

namespace UserSegmentationService.API;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddInfrastructure(_configuration);

        services.AddEndpointsApiExplorer();

    }

    public void Configure(
        WebApplication app,
        IWebHostEnvironment env)
    {

        app.UseAuthorization();

        app.MapControllers();
    }
}