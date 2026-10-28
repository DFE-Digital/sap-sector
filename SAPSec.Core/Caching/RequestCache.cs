using Microsoft.AspNetCore.Http;

namespace SAPSec.Core.Caching;

public class RequestCache(IHttpContextAccessor contextAccessor) : ICache
{
    public async Task<TItem> TryGet<TItem>(string key, Func<Task<TItem>> getItem)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext.Items.TryGetValue(key, out var itemObject) && itemObject is TItem cachedItem)
        {
            return cachedItem;
        }

        var item = await getItem();
        httpContext.Items[key] = item;

        return item;
    }
}