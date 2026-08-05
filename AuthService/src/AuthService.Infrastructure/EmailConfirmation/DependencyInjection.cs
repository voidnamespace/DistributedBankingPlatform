using AuthService.Application.Abstractions.EmailConfirmation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.EmailConfirmation;

internal static class DependencyInjection
{
    internal static IServiceCollection AddEmailConfirmation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSenderOptions>(
            configuration.GetSection(EmailSenderOptions.SectionName));

        services.AddSingleton<IEmailConfirmationTokenStore,
            RedisEmailConfirmationTokenStore>();
        services.AddSingleton<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
