using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsFilters(IDictionary<string, IEnumerable<string>> filterValues, SimilarSchool currentSchool)
{
    private Dictionary<string, ISimilarSchoolsFilter> _filters = new(StringComparer.InvariantCultureIgnoreCase)
    {
        ["Distance"] = new SimilarSchoolsDistanceFilter(currentSchool),
        ["UrbanRural"] = new SimilarSchoolsUrbanRuralFilter(),
    };

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var filteredItems = items;
        foreach (var (key, values) in filterValues)
        {
            if (_filters.ContainsKey(key))
            {
                filteredItems = _filters[key] switch
                {
                    ISimilarSchoolsMultiValueFilter mvf => mvf.Filter(filteredItems, values),
                    ISimilarSchoolsSingleValueFilter svf => svf.Filter(filteredItems, values.LastOrDefault()),
                    _ => filteredItems
                };
            }
        }

        return filteredItems;
    }

    public IDictionary<string, IEnumerable<FilterOption>> GetPossibleOptions(IEnumerable<SimilarSchool> items)
    {
        var possibleOptions = new Dictionary<string, IEnumerable<FilterOption>>();

        foreach (var (key, filter) in _filters)
        {
            var values = filterValues.ContainsKey(key) ? filterValues[key] : [];

            possibleOptions[key] = _filters[key] switch
            {
                ISimilarSchoolsMultiValueFilter mvf => mvf.GetPossibleOptions(items, values),
                ISimilarSchoolsSingleValueFilter svf => svf.GetPossibleOptions(items, values.LastOrDefault()),
                _ => []
            };
        }

        return possibleOptions;
    }
}