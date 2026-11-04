using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedExclusionsRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IExclusionsRepository repository)
    : CachedRepository<IExclusionsRepository>(cache, repository), IExclusionsRepository
{
    public Task<ExclusionsData?> GetByUrnAsync(string urn)
        => CallCachedMethod(r => r.GetByUrnAsync(urn));

    public Task<IReadOnlyCollection<ExclusionsData>> GetByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetByUrnsAsync(urns));
}