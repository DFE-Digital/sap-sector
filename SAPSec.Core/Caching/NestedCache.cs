namespace SAPSec.Core.Caching;

/// <summary>
/// Implementation of <see cref="ICache"/> that uses two other cache implementations to provide two
/// distinct levels of caching. The outer cache is first checked, and on a cache miss the inner cache
/// is then checked. This is indended to be used for when a cache implementation still has some overhead
/// e.g. a network request, so the outer cache can further optimise requests for same cache value.
/// </summary>
/// <typeparam name="TOuterCache">The cache to be used for the outer layer of caching e.g. a memory or HTTP request cache</typeparam>
/// <typeparam name="TInnerCache">The cache to be used for the inner layer of caching e.g. Redis or other network-based cache</typeparam>
/// <param name="outerCache">The outer cache</param>
/// <param name="innerCache">The inner cache</param>
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
