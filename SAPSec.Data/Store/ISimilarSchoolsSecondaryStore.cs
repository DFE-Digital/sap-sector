using SAPSec.Data.Dto.SimilarSchools.Secondary;

namespace SAPSec.Data.Store;

public interface ISimilarSchoolsSecondaryStore
{
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns);
    Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetStandardDeviationsAsync();
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
