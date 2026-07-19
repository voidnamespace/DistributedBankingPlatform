using AuthService.Application.Abstractions.Authentication;
using StackExchange.Redis;

namespace AuthService.Infrastructure.Authentication.TokenBlacklisting;

public sealed class RedisTokenBlacklistStore : ITokenBlacklistStore
{
    private const string KeyPrefix = "authservice:blacklist:access-token:";
    private const string BlacklistMarker = "true";

    private readonly IDatabase _database;

    public RedisTokenBlacklistStore(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public Task BlacklistAsync(
        string tokenId,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        return _database.StringSetAsync(
            BuildKey(tokenId),
            BlacklistMarker,
            expiry: expiresIn,
            when: When.Always,
            flags: CommandFlags.None);
    }

    public Task<bool> IsBlacklistedAsync(
        string tokenId,
        CancellationToken cancellationToken)
    {
        return _database.KeyExistsAsync(
            BuildKey(tokenId),
            CommandFlags.None);
    }

    private static RedisKey BuildKey(string tokenId)
        => $"{KeyPrefix}{tokenId}";
}
