
using SAPSec.Core.Caching;

namespace SAPSec.Web.Caching;

/// <summary>
/// Implementation of <see cref="ICache"/> that caches values in the current <see cref="HttpContext"/> using the
/// <see cref="HttpContext.Items"/> collection. This is used as a small performance enhanceement to ensure an 
/// expensive-to-generate object is only retrieved once per HTTP request. Commonly used in combination with
/// <see cref="NestedCache{TOuterCache, TInnerCache}"/> to reduce network requests e.g. when looking up the same
/// cached value in Redis. No caching is performed if there is no current HTTP context.s
/// </summary>
/// <param name="contextAccessor">The HTTP context accessor</param>
public class HttpRequestCache(IHttpContextAccessor contextAccessor) : ICache
{
    public async Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var httpContext = contextAccessor.HttpContext;

        // If HttpContext is null (e.g. on application startup) bypass cache altogether
        if (httpContext is null)
        {
            return await getValue();
        }

        if (httpContext.Items.TryGetValue(key, out var item) && item is TValue cachedValue)
        {
            return cachedValue;
        }

        var value = await getValue();
        httpContext.Items[key] = value;

        return value;
    }

    public Task Clear()
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        httpContext.Items.Clear();

        return Task.CompletedTask;
    }
}