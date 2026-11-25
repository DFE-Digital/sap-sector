namespace SAPSec.Core.Caching;

public class PassThroughCache : ICache
{
    public Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue) => getValue();
    public Task Clear() => Task.CompletedTask;
}