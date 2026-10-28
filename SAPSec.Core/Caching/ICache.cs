namespace SAPSec.Core.Caching;

public interface ICache
{
    Task<TItem> TryGet<TItem>(string key, Func<Task<TItem>> getItem);
}
