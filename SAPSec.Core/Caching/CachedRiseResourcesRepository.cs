using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedRiseResourcesRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IRiseResourcesRepository repository)
    : CachedRepository<IRiseResourcesRepository>(cache, repository), IRiseResourcesRepository
{
    public Task<RiseResourcesDocument> GetAsync()
        => CallCachedMethod(r => r.GetAsync());
}