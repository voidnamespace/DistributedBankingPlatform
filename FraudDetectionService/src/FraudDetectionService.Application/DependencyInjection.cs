using FraudDetectionService.Application.FraudChecks;
using Microsoft.Extensions.DependencyInjection;

namespace FraudDetectionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFraudCheckService, FraudCheckService>();

        return services;
    }
}
