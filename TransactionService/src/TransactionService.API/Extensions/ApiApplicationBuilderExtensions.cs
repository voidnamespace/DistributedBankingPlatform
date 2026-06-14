using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TransactionService.API.Authentication;

namespace TransactionService.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerConfiguration();

        var loadTestingEnabled = configuration.GetValue<bool>("LoadTesting:Enabled");

        if (loadTestingEnabled)
        {
            services.AddAuthentication(LoadTestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, LoadTestAuthenticationHandler>(
                    LoadTestAuthenticationHandler.SchemeName,
                    options => { });
        }
        else
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)
                        )
                    };
                });
        }

        return services;
    }
}
