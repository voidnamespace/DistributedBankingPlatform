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
        services.AddSwaggerGen();
    }

    public void Configure(
        WebApplication app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();
    }
}
