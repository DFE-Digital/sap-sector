using SAPSec.Core.DataPoints;
using SAPSec.Core.School.Similarity;

namespace SAPSec.Core.School.Similarity.Filtering;

public abstract class SimilarSchoolsFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : ISimilarSchoolsFilter
{
    private readonly Dictionary<string, IEnumerable<string>> _filterValues = filterValues
        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

    protected IDictionary<string, IEnumerable<string>> FilterValues => _filterValues;
    protected SimilarSchool CurrentSchool => currentSchool;
    protected abstract DataWithAvailability<string>? CurrentSchoolValue { get; }
    protected bool HasFilterValues(string key)
        => FilterValues.ContainsKey(key) && FilterValues[key].Any(v => !string.IsNullOrWhiteSpace(v));

    public string Key => key;
    public string Name => name;
    public abstract bool IsApplied { get; }
    public abstract IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items);
    public abstract SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items);
}
