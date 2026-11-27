namespace SAPSec.Core.Caching;

public interface ICache
{
    Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue);
    Task Clear();
}
