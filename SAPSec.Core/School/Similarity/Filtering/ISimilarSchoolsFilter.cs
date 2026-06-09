using SAPSec.Core.School.Similarity;

namespace SAPSec.Core.School.Similarity.Filtering;

public interface ISimilarSchoolsFilter
{
    string Key { get; }
    string Name { get; }
    bool IsApplied { get; }
    IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items);
    SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items);
}
