using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedKs4DestinationsRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IKs4DestinationsRepository repository)
    : CachedRepository<IKs4DestinationsRepository>(cache, repository), IKs4DestinationsRepository
{
    public Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetByUrnsAsync(urns));

    Task<Ks4DestinationsData?> IMeasureDataRepository<Ks4DestinationsData>.GetByUrnAsync(string urn)
        => CallCachedMethod(r => r.GetByUrnAsync(urn));
}
