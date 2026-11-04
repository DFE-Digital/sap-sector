using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core;
using SAPSec.Core.Caching;

namespace SAPSec.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisConfiguration>(configuration.GetSection(nameof(RedisConfiguration)));
        services.AddKeyedSingleton<ICache, RedisCache>(ServiceKeys.RealImplementation);
    }
}
