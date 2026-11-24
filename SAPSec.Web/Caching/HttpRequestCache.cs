
using SAPSec.Core.Caching;

namespace SAPSec.Web.Caching;

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