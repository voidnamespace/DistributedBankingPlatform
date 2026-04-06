using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Caching;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RedisService> _logger;

    private readonly string _prefix = "authservice:";

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private string BuildKey(string key)
        => $"{_prefix}{key}";

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var redisKey = BuildKey(key);

            if (value == null)
            {
                _logger.LogDebug("Redis delete because value is null. Key={Key}", redisKey);

                await _db.KeyDeleteAsync(redisKey);
                return;
            }

            var json = JsonSerializer.Serialize(value, _jsonOptions);

            await _db.StringSetAsync(
                redisKey,
                json,
                expiry: expiry,
                when: When.Always,
                flags: CommandFlags.None
            );

            _logger.LogDebug("Redis SET success. Key={Key}", redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SetAsync failed. Key={Key}", key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var redisKey = BuildKey(key);

            var value = await _db.StringGetAsync(redisKey);

            if (!value.HasValue)
            {
                _logger.LogDebug("Redis GET miss. Key={Key}", redisKey);
                return default;
            }

            _logger.LogDebug("Redis GET hit. Key={Key}", redisKey);

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GetAsync failed. Key={Key}", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);

            await _db.KeyDeleteAsync(redisKey);

            _logger.LogDebug("Redis REMOVE success. Key={Key}", redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis RemoveAsync failed. Key={Key}", key);
            throw;
        }
    }
}
