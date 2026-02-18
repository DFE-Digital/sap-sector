using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsNumericRangeFilter : ISimilarSchoolsFilter
{
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? from, string? to);
    SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, string? from, string? to);
}