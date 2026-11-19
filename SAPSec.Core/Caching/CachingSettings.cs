namespace SAPSec.Core.Caching;

public class CachingSettings
{
    public bool EnableCaching { get; set; }
    public bool CacheUseCases { get; set; }
    public bool CacheRepositories { get; set; }
    public bool ClearCacheOnStartup { get; set; }
}
