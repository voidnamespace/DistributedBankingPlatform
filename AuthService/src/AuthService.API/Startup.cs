using AspNetCoreRateLimit;
using AuthService.API.Extensions;
using AuthService.API.Middleware;
using AuthService.Application;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Seeding;
using OpenTelemetry.Metrics;
using Microsoft.EntityFrameworkCore;

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
        services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter();
    });

        services.AddApplication();
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
        app.MapPrometheusScrapingEndpoint();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseIpRateLimiting();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "AuthService API V1");
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
