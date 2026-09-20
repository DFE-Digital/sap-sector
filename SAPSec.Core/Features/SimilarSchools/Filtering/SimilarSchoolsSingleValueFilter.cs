using SAPSec.Core.Collections;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsSingleValueFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    public override bool IsApplied => HasFilterValues(Key);

    public override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor)
    {
        var value = (FilterValues.ContainsKey(Key) ? FilterValues[Key] : []).LastOrDefault();

        return Filter(items, similarSchoolAccessor, value);
    }

    public override SimilarSchoolsAvailableFilter? AsAvailableFilter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor)
    {
        var value = (FilterValues.ContainsKey(Key) ? FilterValues[Key] : []).LastOrDefault();
        var options = GetPossibleOptions(items, similarSchoolAccessor, value).ToList().AsReadOnly();
        if (options.Count > 1)
        {
            return new SimilarSchoolsSingleValueAvailableFilter(
                Key,
                Name,
                options,
                CurrentSchoolValue);
        }

        return null;
    }

    protected abstract IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, string? value);
    protected abstract IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, string? value);
}
