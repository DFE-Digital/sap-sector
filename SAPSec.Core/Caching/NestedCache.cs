namespace SAPSec.Core.Caching;

public class NestedCache(ICache outerCache, ICache innerCache) : ICache
{
    public Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
        => outerCache.TryGet(key, () => innerCache.TryGet(key, getValue));

    public async Task Clear()
    {
        await innerCache.Clear();
        await outerCache.Clear();
    }
}
