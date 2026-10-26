namespace SAPSec.Core;

public class InMemoryCache : ICache
{
    private Dictionary<string, object?> _store = new();

    public async Task<TItem> TryGet<TItem>(string key, Func<Task<TItem>> getItem)
    {
        if (_store.TryGetValue(key, out var itemObject) && itemObject is TItem cachedItem)
        {
            return cachedItem;
        }

        var item = await getItem();
        _store[key] = item;

        return item;
    }
}
