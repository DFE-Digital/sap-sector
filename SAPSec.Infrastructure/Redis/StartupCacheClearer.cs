using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SAPSec.Core.Caching;

namespace SAPSec.Infrastructure.Redis;

public class StartupCacheClearer(
    ICache cache,
    IOptions<CachingConfiguration> config)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (config.Value.ClearCacheOnStartup)
        {
            await cache.Clear();
        }
    }
}
