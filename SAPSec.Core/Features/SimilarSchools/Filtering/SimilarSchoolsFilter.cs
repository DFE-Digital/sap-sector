using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

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

    public string Key => key;
    public string Name => name;
    public abstract bool IsApplied { get; }
    public abstract IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items);
    public abstract SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items);
}
