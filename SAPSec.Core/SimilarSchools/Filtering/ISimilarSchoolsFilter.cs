using SAPSec.Core.SimilarSchools;

namespace SAPSec.Core.SimilarSchools.Filtering;

public interface ISimilarSchoolsFilter
{
    string Key { get; }
    string Name { get; }
    bool IsApplied { get; }
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items);
    SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items);
}
