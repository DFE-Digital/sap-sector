namespace SAPSec.Core.Caching;

public class NestedCache<TOuterCache, TInnerCache>(TOuterCache outerCache, TInnerCache innerCache)
    : ICache
    where TOuterCache : class, ICache
    where TInnerCache : class, ICache
{
    public Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
        => outerCache.TryGet(key, () => innerCache.TryGet(key, getValue));

    public async Task Clear()
    {
        await innerCache.Clear();
        await outerCache.Clear();
    }
}
