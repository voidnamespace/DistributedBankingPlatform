using AccountService.API.Extensions;
using AccountService.Application;
using AccountService.Infrastructure.Data;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using AccountService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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


        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                 {
                     var key = Configuration["Jwt:SecretKey"]
                      ?? throw new Exception("JWT key not configured");

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                      ValidateIssuer = true,
                      ValidIssuer = Configuration["Jwt:Issuer"],

                      ValidateAudience = true,
                      ValidAudience = Configuration["Jwt:Audience"],

                      ValidateLifetime = true,

                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey =
                      new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                 };
             });

        services.AddAuthorization();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();

        app.UseSwaggerConfiguration();
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "AuthService API V1");
                c.RoutePrefix = string.Empty;
            });
        }
        app.UseIpRateLimiting();

        app.UseAuthentication();
        app.UseAuthorization();

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