using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsMultiValueFilter : ISimilarSchoolsFilter
{
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values);
    SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values);
}
