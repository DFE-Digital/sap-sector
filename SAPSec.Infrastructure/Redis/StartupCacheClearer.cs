using Microsoft.Extensions.Hosting;
using SAPSec.Core.Caching;

namespace SAPSec.Infrastructure.Redis;

public class StartupCacheClearer(
    ICache cache)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await cache.Clear();
    }
}
