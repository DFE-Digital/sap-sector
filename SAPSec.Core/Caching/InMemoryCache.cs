using System.Text.Json;

namespace SAPSec.Core.Caching;

/// <summary>
/// In-memory implementation of <see cref="ICache"/> - intended for use in automated tests but could also be used on Test or Production
/// to replace Redis cache if needed. 
/// Note: this deliberately serializes and deserializes cache values as although this is slower and uses more memory, we want to
/// surface any deserialization issues with use case responses in automated tests during development, before hitting the Redis cache
/// on Test or Production
/// </summary>
public class InMemoryCache : ICache
{
    private Dictionary<string, string?> _store = new();

    public async Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
    {
        if (_store.TryGetValue(key, out var json))
        {
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
        }

        var value = await getValue();
        _store[key] = JsonSerializer.Serialize(value);

        return value;
    }

    public Task Clear()
    {
        _store.Clear();

        return Task.CompletedTask;
    }

    public List<TValue> GetAllValues<TValue>(string keyPrefix = "") =>
        _store
            .Where(kvp => kvp.Key.StartsWith(keyPrefix))
            .Select(kvp => JsonSerializer.Deserialize<TValue>(kvp.Value ?? "null"))
            .OfType<TValue>()
            .ToList();

    public List<string> GetAllKeys(string keyPrefix = "") =>
        _store.Keys
            .Where(k => k.StartsWith(keyPrefix))
            .ToList();
}
