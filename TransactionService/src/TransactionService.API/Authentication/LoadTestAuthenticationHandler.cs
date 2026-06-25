using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TransactionService.API.Authentication;

public class LoadTestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "LoadTest";
    public const string UserIdHeaderName = "X-Test-User-Id";

    public LoadTestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeaderName, out var values))
        {
            return Task.FromResult(
                AuthenticateResult.Fail($"Missing {UserIdHeaderName} header"));
        }

        var userId = values.FirstOrDefault();

        if (!Guid.TryParse(userId, out _))
        {
            return Task.FromResult(
                AuthenticateResult.Fail($"Invalid {UserIdHeaderName} header"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId!)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
