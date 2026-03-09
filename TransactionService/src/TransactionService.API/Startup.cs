using TransactionService.Application.Commands.CreateTransfer;
using TransactionService.Infrastructure.Extensions;

namespace TransactionService.API;

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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreateTransferCommand).Assembly));
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }



}
