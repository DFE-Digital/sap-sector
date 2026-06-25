using SAPSec.Data.Dto.SimilarSchools.Primary;

namespace SAPSec.Data.Store;

public interface ISimilarSchoolsPrimaryStore
{
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
