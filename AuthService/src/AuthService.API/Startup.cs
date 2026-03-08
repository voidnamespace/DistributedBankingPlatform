using AspNetCoreRateLimit;
using AuthService.API.Extensions;
using AuthService.Application.Commands.RegisterUser;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using AuthService.Application.EventHandlers;

namespace AuthService.API;

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
    cfg.RegisterServicesFromAssemblies(
        typeof(RegisterUserCommand).Assembly,
        typeof(UserActivatedDomainEventHandler).Assembly
    ));

        services.AddApi(Configuration);
        services.AddInfrastructure(Configuration);
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Database.Migrate(); 

            var seeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();
            seeder.SeedAsync().GetAwaiter().GetResult(); 
        }
 
        app.UseMiddleware<AuthService.Infrastructure.Middleware.ExceptionMiddleware>();
        app.UseIpRateLimiting();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API V1");
                c.RoutePrefix = string.Empty; 
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthCheckEndpoints();

        app.Logger.LogInformation("AuthService started successfully");
    }
}
