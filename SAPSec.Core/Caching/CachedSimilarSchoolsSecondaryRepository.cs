using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedSimilarSchoolsSecondaryRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] ISimilarSchoolsSecondaryRepository repository)
    : CachedRepository<ISimilarSchoolsSecondaryRepository>(cache, repository), ISimilarSchoolsSecondaryRepository
{
    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => CallCachedMethod(r => r.GetAllUrnsInSimilarSchoolsDataSet());

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetGroupAsync(string urn)
        => CallCachedMethod(r => r.GetGroupAsync(urn));

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetValuesByUrnsAsync(urns));
}
