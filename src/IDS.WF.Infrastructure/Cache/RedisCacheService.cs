using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.Infrastructure.Cache;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<RedisCacheService> _logger = logger;
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value.ToString()) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(60));
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length == 0) return;

            // NOTE: In clustered/replicated setups you'll usually need to iterate all endpoints.
            var server = _redis.GetServer(endpoints[0]);
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Length > 0)
                await _db.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed removing Redis keys by prefix {Prefix}", prefix);
        }
    }
}

