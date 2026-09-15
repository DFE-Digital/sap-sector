using SAPSec.Core.Collections;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsMultiValueFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    public override bool IsApplied => HasFilterValues(Key);

    public override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor)
    {
        var values = FilterValues.ContainsKey(Key) ? FilterValues[Key] : [];

        return Filter(items, similarSchoolAccessor, values);
    }

    public override SimilarSchoolsAvailableFilter? AsAvailableFilter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor)
    {
        var values = FilterValues.ContainsKey(Key) ? FilterValues[Key] : [];
        var options = GetPossibleOptions(items, similarSchoolAccessor, values).ToList().AsReadOnly();
        if (options.Count > 1)
        {
            return new SimilarSchoolsMultiValueAvailableFilter(
                Key,
                Name,
                options,
                CurrentSchoolValue);
        }

        return null;
    }

    protected abstract IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values);
    protected abstract IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values);
}
