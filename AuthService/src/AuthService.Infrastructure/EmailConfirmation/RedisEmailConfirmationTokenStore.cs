using AuthService.Application.Abstractions.EmailConfirmation;
using StackExchange.Redis;

namespace AuthService.Infrastructure.EmailConfirmation;

public sealed class RedisEmailConfirmationTokenStore
    : IEmailConfirmationTokenStore
{
    private const string KeyPrefix = "authservice:email-confirmation:";

    private readonly IDatabase _database;

    public RedisEmailConfirmationTokenStore(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public Task StoreAsync(
        string tokenHash,
        Guid userId,
        TimeSpan lifetime,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _database.StringSetAsync(
            BuildKey(tokenHash),
            userId.ToString(),
            expiry: lifetime,
            when: When.Always,
            flags: CommandFlags.None);
    }

    public async Task<Guid?> GetUserIdAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var storedUserId = await _database.StringGetAsync(
            BuildKey(tokenHash),
            CommandFlags.None);

        if (storedUserId.IsNullOrEmpty)
            return null;

        return Guid.TryParse(storedUserId.ToString(), out var userId)
            ? userId
            : null;
    }

    public async Task DeleteAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _database.KeyDeleteAsync(
            BuildKey(tokenHash),
            CommandFlags.None);
    }

    private static RedisKey BuildKey(string tokenHash)
        => $"{KeyPrefix}{tokenHash}";
}
