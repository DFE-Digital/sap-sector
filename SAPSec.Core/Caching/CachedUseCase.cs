using SAPSec.Core.UseCases;
using System.Security.Cryptography;
using System.Text.Json;

namespace SAPSec.Core.Caching;

/// <summary>
/// Decorator over <see cref="IUseCase{TRequest, TResponse}"/> that caches the use case response object
/// against a hashed version of the request. Note that this requires the response of the decorated use case to be 
/// fully determinined by its request for any given version of the underlying data. Care should be taken when caching
/// functionality that non-deterministic, e.g. that depends on the current time, random number generation, user-specific
/// data etc. Response objects should also be serializable.
/// </summary>
/// <typeparam name="TRequest">The use case request object</typeparam>
/// <typeparam name="TResponse">The response for the given request</typeparam>
/// <param name="useCase">The underlying use case that is decorated</param>
/// <param name="cache">The cache implementation</param>
public class CachedUseCase<TRequest, TResponse>(IUseCase<TRequest, TResponse> useCase, ICache cache)
    : IUseCase<TRequest, TResponse>
{
    public async Task<TResponse> Execute(TRequest request)
    {
        var key = JsonSerializer.Serialize(request);
        var hash = Convert.ToHexString(SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(key)));

        var cachedResponse = await cache.TryGet($"UseCase::{useCase.GetType().Name}::{hash}", () => useCase.Execute(request));

        if (cachedResponse is null)
        {
            throw new ApplicationException($"Cached response for use case {useCase.GetType().FullName} was null");
        }

        return cachedResponse;
    }
}
