namespace SAPSec.Core.Caching;

/// <summary>
/// Implementation of <see cref="ICache"/> that does nothing - this is used when caching is turned off but an implementation
/// of <see cref="ICache"/> is still expected, e.g. in automated tests when cleardown is done after every test, or
/// <see cref="CachedUseCase{TRequest, TResponse}"/> which expects an <see cref="ICache"/> instance.
/// </summary>
public class PassThroughCache : ICache
{
    public Task<TValue> TryGet<TValue>(string key, Func<Task<TValue>> getValue) => getValue();
    public Task Clear() => Task.CompletedTask;
}