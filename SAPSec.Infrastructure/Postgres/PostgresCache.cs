using Dapper;
using SAPSec.Core.Caching;
using System.Text.Json;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresCache(NpgsqlDataSourceFactory factory) : ICache
{
    public async Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
    {
        using var conn = await factory.Create().OpenConnectionAsync();

        var json = await conn.QuerySingleOrDefaultAsync<string?>("SELECT value FROM cache WHERE key = @key",
            new { key });

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
        await conn.ExecuteAsync("INSERT INTO cache VALUES(@key, @value::json)",
            new { key, value = JsonSerializer.Serialize(value) });

        return value;
    }

    public async Task Clear()
    {
        using var conn = await factory.Create().OpenConnectionAsync();
        await conn.ExecuteAsync("TRUNCATE TABLE cache");
    }
}
