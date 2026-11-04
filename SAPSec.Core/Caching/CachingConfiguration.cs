namespace SAPSec.Core.Caching;

public class CachingConfiguration
{
    public bool EnableCaching { get; set; }
    public bool CacheUseCases { get; set; }
    public bool CacheRepositories { get; set; }
    public bool ClearCacheOnStartup { get; set; }
}
