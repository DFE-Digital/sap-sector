using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsSingleValueFilter : ISimilarSchoolsFilter
{
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? value);
    SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, string? value);
}