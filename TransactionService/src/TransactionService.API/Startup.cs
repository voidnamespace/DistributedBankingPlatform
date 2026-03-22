using Microsoft.EntityFrameworkCore;
using TransactionService.API.Extensions;
using TransactionService.Application.Commands.CreateTransfer;
using TransactionService.Application.EventHandlers;
using TransactionService.Infrastructure.Data;
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
        services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssemblies(
            typeof(CreateTransferCommand).Assembly,
            typeof(TransferCreatedDomainEventHandler).Assembly
        ));
        services.AddApi(Configuration);
    }
    

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            db.Database.Migrate();
        }
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TransactionService API V1");
                c.RoutePrefix = string.Empty;
            });
        }
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }

}
