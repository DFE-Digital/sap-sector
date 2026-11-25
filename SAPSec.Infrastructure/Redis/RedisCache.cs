using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SAPSec.Core.Caching;
using StackExchange.Redis;
using System.Text.Json;

namespace SAPSec.Infrastructure.Redis;

/// <summary>
/// Redis implementation of <see cref="ICache"/>. 
/// </summary>
/// <param name="settings">Settings object containing the Redis connection string</param>
/// <param name="logger">Logger object</param>
public class RedisCache(IOptions<RedisSettings> settings, ILogger<RedisCache> logger) : ICache, IDisposable
{
    private ConnectionMultiplexer _conn = ConnectionMultiplexer.Connect(settings.Value.ConnectionString);

    public void Dispose()
    {
        _conn.Dispose();
    }

    public async Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var db = _conn.GetDatabase();

        var json = (string?)(await db.StringGetAsync(key));

        try
        {
            if (json is not null && JsonSerializer.Deserialize<TValue>(json) is TValue cachedValue)
            {
                return cachedValue;
            }
        }
        catch (JsonException ex)
        {
            throw new ApplicationException($"Error deserializing cached value for key {key} to type {typeof(TValue).Name}: {ex.Message}{Environment.NewLine} {json}", ex);
        }

        var value = await getValue();
        await db.StringSetAsync(key, JsonSerializer.Serialize(value));

        return value;
    }

    public async Task Clear()
    {
        var db = _conn.GetDatabase();
        foreach (var server in _conn.GetServers())
        {
            await foreach (var key in server.KeysAsync())
            {
                logger.LogInformation("Deleting key {Key}", key);
                await db.KeyDeleteAsync(key);
            }
        }
    }
}
