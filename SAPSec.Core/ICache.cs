namespace SAPSec.Core;

public interface ICache
{
    Task<TItem> TryGet<TItem>(string key, Func<Task<TItem>> getItem);
}
