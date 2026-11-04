using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedKs2PerformanceRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IKs2PerformanceRepository repository)
    : CachedRepository<IKs2PerformanceRepository>(cache, repository), IKs2PerformanceRepository
{
    public Task<IReadOnlyCollection<Ks2PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetByUrnsAsync(urns));

    Task<Ks2PerformanceData?> IMeasureDataRepository<Ks2PerformanceData>.GetByUrnAsync(string urn)
        => CallCachedMethod(r => r.GetByUrnAsync(urn));
}
