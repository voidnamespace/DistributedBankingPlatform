using EventProjectionService.Infrastructure.Extensions;

namespace EventProjectionService.API;

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
    }


    public void Configure(WebApplication app, IWebHostEnvironment env)
    {


    }



    }