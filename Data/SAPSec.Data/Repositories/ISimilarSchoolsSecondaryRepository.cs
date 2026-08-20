using SAPSec.Data.Dto.SimilarSchools.Secondary;

namespace SAPSec.Data.Repositories;

public interface ISimilarSchoolsSecondaryRepository
{
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
