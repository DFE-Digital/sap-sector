namespace SAPSec.Core.Caching;

public class NestedCache(ICache outerCache, ICache innerCache) : ICache
{
    public Task<TItem> TryGet<TItem>(string key, Func<Task<TItem>> getItem)
        => outerCache.TryGet(key, () => innerCache.TryGet(key, getItem));
}
