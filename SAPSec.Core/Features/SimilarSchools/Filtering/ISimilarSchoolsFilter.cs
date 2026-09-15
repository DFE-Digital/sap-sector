using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public interface ISimilarSchoolsFilter
{
    string Key { get; }
    string Name { get; }
    bool IsApplied { get; }
    IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor);
    SimilarSchoolsAvailableFilter? AsAvailableFilter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor);
}
