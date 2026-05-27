using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Caching;

public sealed class RedisTokenBlacklistStore : ITokenBlacklistStore
{
    private readonly IRedisService _redis;

    public RedisTokenBlacklistStore(IRedisService redis)
    {
        _redis = redis;
    }

    public Task BlacklistAsync(
        string tokenId,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        return _redis.SetAsync(
            $"blacklist:access-token:{tokenId}",
            true,
            expiresIn);
    }

    public async Task<bool> IsBlacklistedAsync(
        string tokenId,
        CancellationToken cancellationToken)
    {
        var value = await _redis.GetAsync<bool>(
            $"blacklist:access-token:{tokenId}");

        return value;
    }
}
