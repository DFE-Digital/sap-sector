using SAPSec.Core.UseCases;
using System.Security.Cryptography;
using System.Text.Json;

namespace SAPSec.Core.Caching;

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
