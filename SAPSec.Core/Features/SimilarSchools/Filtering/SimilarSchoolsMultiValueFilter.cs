using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsMultiValueFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    public override bool IsApplied => HasFilterValues(Key);

    public override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var values = FilterValues.ContainsKey(Key) ? FilterValues[Key] : [];

        return Filter(items, values);
    }

    public override SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items)
    {
        var values = FilterValues.ContainsKey(Key) ? FilterValues[Key] : [];
        var options = GetPossibleOptions(items, values).ToList().AsReadOnly();
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

    protected abstract IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values);
    protected abstract IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values);
}
