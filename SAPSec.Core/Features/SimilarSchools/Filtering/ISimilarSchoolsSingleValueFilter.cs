using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsSingleValueFilter
{
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? value);
    IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, string? value);
}