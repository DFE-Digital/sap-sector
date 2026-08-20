using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsFilter
{
    string Key { get; }
    string Name { get; }
    bool IsApplied { get; }
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items);
    SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items);
}
