using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedSimilarSchoolsPrimaryRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] ISimilarSchoolsPrimaryRepository repository)
    : CachedRepository<ISimilarSchoolsPrimaryRepository>(cache, repository), ISimilarSchoolsPrimaryRepository
{
    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => CallCachedMethod(r => r.GetAllUrnsInSimilarSchoolsDataSet());

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
        => CallCachedMethod(r => r.GetGroupAsync(urn));

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetValuesByUrnsAsync(urns));
}
