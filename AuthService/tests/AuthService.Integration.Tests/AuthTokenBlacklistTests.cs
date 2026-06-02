using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Integration.Tests;

public sealed class AuthTokenBlacklistTests
    : IClassFixture<AuthServiceIntegrationFactory>
{
    private readonly HttpClient _client;

    public AuthTokenBlacklistTests(AuthServiceIntegrationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Logout_WithValidAccessToken_ShouldRejectSameTokenAfterLogout()
    {
        var email = $"blacklist-{Guid.NewGuid():N}@local.dev";
        const string password = "StrongPass123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                email,
                password,
                confirmPassword = password
            });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                email,
                password
            });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        login.Should().NotBeNull();
        login!.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var authenticatedResponse = await _client.GetAsync("/api/Auth/me");

        authenticatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutResponse = await _client.PostAsync("/api/Auth/logout", content: null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var revokedTokenResponse = await _client.GetAsync("/api/Auth/me");

        revokedTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
