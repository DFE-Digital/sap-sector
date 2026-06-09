using SAPSec.Core.Filtering;
using SAPSec.Core.School.Similarity;

namespace SAPSec.Core.School.Similarity.Filtering;

public abstract class SimilarSchoolsSingleValueFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    public override bool IsApplied => HasFilterValues(Key);

    public override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var value = (FilterValues.ContainsKey(Key) ? FilterValues[Key] : []).LastOrDefault();

        return Filter(items, value);
    }

    public override SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items)
    {
        var value = (FilterValues.ContainsKey(Key) ? FilterValues[Key] : []).LastOrDefault();
        var options = GetPossibleOptions(items, value).ToList().AsReadOnly();
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

    protected abstract IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? value);
    protected abstract IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, string? value);
}
