using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedKs4PerformanceRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IKs4PerformanceRepository repository)
    : CachedRepository<IKs4PerformanceRepository>(cache, repository), IKs4PerformanceRepository
{
    public Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetByUrnsAsync(urns));

    Task<Ks4PerformanceData?> IMeasureDataRepository<Ks4PerformanceData>.GetByUrnAsync(string urn)
        => CallCachedMethod(r => r.GetByUrnAsync(urn));
}
