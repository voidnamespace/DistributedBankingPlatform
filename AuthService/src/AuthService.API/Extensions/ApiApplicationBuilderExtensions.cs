using FluentValidation.AspNetCore;

namespace AuthService.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerConfiguration();

        services.AddCorsConfiguration();

        services.AddRateLimitingConfiguration(configuration);

        services.AddHealthChecksConfiguration(configuration);

        return services;
    }
}
