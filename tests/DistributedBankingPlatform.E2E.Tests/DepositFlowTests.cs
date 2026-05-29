using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DistributedBankingPlatform.E2E.Tests;

public sealed class DepositFlowTests
{
    private static readonly Uri GatewayBaseUrl = new(
        Environment.GetEnvironmentVariable("DBP_GATEWAY_BASE_URL")
        ?? "http://localhost:5086");

    private readonly HttpClient _client = new()
    {
        BaseAddress = GatewayBaseUrl
    };

    [Fact]
    public async Task RegisteredUser_ShouldBeAbleToDepositMoney()
    {
        const int copperCurrency = 2;
        const decimal depositAmount = 100m;
        var email = $"deposit-{Guid.NewGuid():N}@local.dev";
        const string password = "StrongPass123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/api/Auth/register",
            new
            {
                email,
                password,
                confirmPassword = password
            });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/api/Auth/login",
            new
            {
                email,
                password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var account = await WaitForAccountAsync();
        var initialBalance = account.BalanceAmount;

        var depositResponse = await _client.PostAsJsonAsync(
            "/transaction/api/Transaction/deposit",
            new
            {
                toAccountNumber = account.AccountNumber,
                amount = depositAmount,
                currency = copperCurrency
            });

        Assert.Equal(HttpStatusCode.Accepted, depositResponse.StatusCode);

        var deposit = await depositResponse.Content.ReadFromJsonAsync<DepositResponse>();

        Assert.NotNull(deposit);
        Assert.NotEqual(Guid.Empty, deposit.TransactionId);

        await WaitForTransactionStatusAsync(deposit.TransactionId, "Completed");

        var updatedAccount = await WaitForAccountBalanceAsync(
            account.AccountNumber,
            initialBalance + depositAmount);

        Assert.Equal(initialBalance + depositAmount, updatedAccount.BalanceAmount);
    }

    private async Task<AccountResponse> WaitForAccountAsync()
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await _client.GetAsync("/account/api/Account/me");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
            var account = accounts?.SingleOrDefault(account => account.BalanceCurrency == 2);

            if (account is not null)
            {
                return account;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException("Account was not created before the E2E timeout expired.");
    }

    private async Task<AccountResponse> WaitForAccountBalanceAsync(
        string accountNumber,
        decimal expectedBalance)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await _client.GetAsync($"/account/api/Account/by-number/{accountNumber}");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var account = await response.Content.ReadFromJsonAsync<AccountResponse>();

            if (account is not null && account.BalanceAmount == expectedBalance)
            {
                return account;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException("Deposit was not reflected in the account balance before the E2E timeout expired.");
    }

    private async Task WaitForTransactionStatusAsync(
        Guid transactionId,
        string expectedStatus)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await _client.GetAsync($"/transaction/api/Transaction/{transactionId}");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var status = await response.Content.ReadAsStringAsync();

            if (status == expectedStatus)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException(
            $"Transaction {transactionId} did not reach {expectedStatus} before the E2E timeout expired.");
    }

    private sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class DepositResponse
    {
        public Guid TransactionId { get; set; }
    }

    private sealed class AccountResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal BalanceAmount { get; set; }
        public int BalanceCurrency { get; set; }
    }
}
