using SAPSec.Data.Model.Generated;

namespace SAPSec.Data;

public interface ISimilarSchoolsSecondaryRepository
{
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn);
    Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(IEnumerable<string> urns);
    Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync();
}
