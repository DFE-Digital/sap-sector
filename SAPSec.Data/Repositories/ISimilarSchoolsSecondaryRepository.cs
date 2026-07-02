using SAPSec.Data.Dto.SimilarSchools.Secondary;

namespace SAPSec.Data.Repositories;

public interface ISimilarSchoolsSecondaryRepository
{
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(IEnumerable<string> urns);
    Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync();
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
