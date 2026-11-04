using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedAbsenceRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IAbsenceRepository repository)
    : CachedRepository<IAbsenceRepository>(cache, repository), IAbsenceRepository
{
    public Task<AbsenceData?> GetByUrnAsync(string urn)
        => CallCachedMethod(r => r.GetByUrnAsync(urn));

    public Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetByUrnsAsync(urns));
}
