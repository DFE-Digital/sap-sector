using Microsoft.AspNetCore.Http;

namespace SAPSec.Core.Caching;

public class RequestCache(IHttpContextAccessor contextAccessor) : ICache
{
    public async Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var httpContext = contextAccessor.HttpContext;
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