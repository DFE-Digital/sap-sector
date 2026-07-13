using SAPSec.Data.Dto.SimilarSchools.Primary;

namespace SAPSec.Data.Repositories;

public interface ISimilarSchoolsPrimaryRepository
{
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetPrimaryValuesByUrnsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
