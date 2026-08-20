using SAPSec.Core.Collections;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;
namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : ISimilarSchoolsFilter
{
    protected CaseInsensitiveDictionary<IEnumerable<string>> FilterValues => filterValues;
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
