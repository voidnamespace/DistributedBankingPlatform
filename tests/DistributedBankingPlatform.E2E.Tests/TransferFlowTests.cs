using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DistributedBankingPlatform.E2E.Tests;

public sealed class TransferFlowTests
{
    private static readonly Uri GatewayBaseUrl = new(
        Environment.GetEnvironmentVariable("DBP_GATEWAY_BASE_URL")
        ?? "http://localhost:5086");

    private readonly HttpClient _senderClient = new()
    {
        BaseAddress = GatewayBaseUrl
    };

    private readonly HttpClient _receiverClient = new()
    {
        BaseAddress = GatewayBaseUrl
    };

    [Fact]
    public async Task RegisteredUser_ShouldBeAbleToTransferMoney_ToOtherRegisteredUser()
    {
        const int copperCurrency = 2;
        const decimal depositAmount = 100m;
        const decimal transferAmount = 100m;

        var senderEmail = $"transfer-sender-{Guid.NewGuid():N}@local.dev";
        const string sendersPassword = "joiruefhojja213123";
        var recieverEmail = $"transfer-reciever-{Guid.NewGuid():N}@local.dev";
        const string recieversPassword = "rjfousf2131245r345t";

        var senderRegisterResponse = await _senderClient.PostAsJsonAsync(
            "/auth/api/Auth/register",
            new
            {
                email = senderEmail,
                password = sendersPassword,
                confirmPassword = sendersPassword
            });

        Assert.Equal(HttpStatusCode.OK, senderRegisterResponse.StatusCode);


        var senderLoginResponse = await _senderClient.PostAsJsonAsync(
            "/auth/api/Auth/login",
                new
                {
                    email = senderEmail,
                    password = sendersPassword
                });

        Assert.Equal(HttpStatusCode.OK, senderLoginResponse.StatusCode);

        var senderLogin = await senderLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(senderLogin);
        Assert.False(string.IsNullOrWhiteSpace(senderLogin.AccessToken));


        _senderClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", senderLogin.AccessToken);

        var senderAccount = await WaitForAccountAsync(_senderClient);

        var recieverRegisterResponse = await _receiverClient.PostAsJsonAsync(
            "/auth/api/Auth/register",
            new
            {
                email = recieverEmail,
                password = recieversPassword,
                confirmPassword = recieversPassword
            });

        Assert.Equal(HttpStatusCode.OK, recieverRegisterResponse.StatusCode);

        var recieverLoginResponse = await _receiverClient.PostAsJsonAsync(
            "/auth/api/Auth/login",
            new
            {
                email = recieverEmail,
                password = recieversPassword
            });

        Assert.Equal(HttpStatusCode.OK, recieverLoginResponse.StatusCode);

        var receiverLogin = await recieverLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(receiverLogin);
        Assert.False(string.IsNullOrWhiteSpace(receiverLogin.AccessToken));

        _receiverClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", receiverLogin.AccessToken);

        var receiverAccount = await WaitForAccountAsync(_receiverClient);
        var senderInitialBalance = senderAccount.BalanceAmount;


        var depositResponse = await _senderClient.PostAsJsonAsync(
            "/transaction/api/Transaction/deposit",
            new
            {
                toAccountNumber = senderAccount.AccountNumber,
                amount = depositAmount,
                currency = copperCurrency
            });

        Assert.Equal(HttpStatusCode.Accepted, depositResponse.StatusCode);

        var deposit = await depositResponse.Content.ReadFromJsonAsync<DepositResponse>();

        Assert.NotNull(deposit);
        Assert.NotEqual(Guid.Empty, deposit.TransactionId);

        await WaitForTransactionStatusAsync(deposit.TransactionId, "Completed");

        var updatedAccount = await WaitForAccountBalanceAsync(
            _senderClient,
            senderAccount.AccountNumber,
            senderInitialBalance + depositAmount);

        Assert.Equal(senderInitialBalance + depositAmount, updatedAccount.BalanceAmount);


        var transferResponse = await _senderClient.PostAsJsonAsync(
            "/transaction/api/Transaction/transfer",
            new
            {
                fromAccountNumber = senderAccount.AccountNumber,
                toAccountNumber = receiverAccount.AccountNumber,
                amount = transferAmount,
                currency = copperCurrency
            });

        Assert.Equal(HttpStatusCode.Accepted, transferResponse.StatusCode);

        var transfer = await transferResponse.Content.ReadFromJsonAsync<TransferResponse>();

        Assert.NotNull(transfer);
        Assert.NotEqual(Guid.Empty, transfer.TransactionId);

        await WaitForTransactionStatusAsync(transfer.TransactionId, "Completed");

        var updatedSenderAccount = await WaitForAccountBalanceAsync(
            _senderClient,
            senderAccount.AccountNumber,
            senderInitialBalance + depositAmount - transferAmount);

        var updatedReceiverAccount = await WaitForAccountBalanceAsync(
            _receiverClient,
            receiverAccount.AccountNumber,
            receiverAccount.BalanceAmount + transferAmount);

        Assert.Equal(
            senderInitialBalance + depositAmount - transferAmount,
            updatedSenderAccount.BalanceAmount);

        Assert.Equal(
            receiverAccount.BalanceAmount + transferAmount,
            updatedReceiverAccount.BalanceAmount);

    }

    private static async Task<AccountResponse> WaitForAccountAsync(HttpClient client)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await client.GetAsync("/account/api/Account/me");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var accounts = await response.Content
                .ReadFromJsonAsync<List<AccountResponse>>();

            var account = accounts?
                .SingleOrDefault(account => account.BalanceCurrency == 2);

            if (account is not null)
            {
                return account;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException(
            "Account was not created before the E2E timeout expired.");
    }

    private async Task WaitForTransactionStatusAsync(
        Guid transactionId,
        string expectedStatus)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await _senderClient.GetAsync($"/transaction/api/Transaction/{transactionId}");

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

    private static async Task<AccountResponse> WaitForAccountBalanceAsync(
        HttpClient client,
        string accountNumber,
        decimal expectedBalance)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var response = await client.GetAsync(
                $"/account/api/Account/by-number/{accountNumber}");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var account = await response.Content
                .ReadFromJsonAsync<AccountResponse>();

            if (account is not null &&
                account.BalanceAmount == expectedBalance)
            {
                return account;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException(
            $"Account {accountNumber} did not reach balance {expectedBalance}.");
    }

    private sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class AccountResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal BalanceAmount { get; set; }
        public int BalanceCurrency { get; set; }
    }

    private sealed class TransferResponse
    {
        public Guid TransactionId { get; set; }
    }

    private sealed class DepositResponse
    {
        public Guid TransactionId { get; set; }
    }

}
