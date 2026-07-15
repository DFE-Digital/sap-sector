using SAPSec.Data.Dto.SimilarSchools.Primary;

namespace SAPSec.Data.Repositories;

public interface ISimilarSchoolsPrimaryRepository
{
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
