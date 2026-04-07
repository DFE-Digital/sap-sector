using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsSingleValueFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    public override bool IsApplied => FilterValues.ContainsKey(Key);

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
