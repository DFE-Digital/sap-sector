using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsFilters(IDictionary<string, IEnumerable<string>> filterValues, SimilarSchool currentSchool)
{
    private Dictionary<string, ISimilarSchoolsFilter> _filters = new List<ISimilarSchoolsFilter>
    {
        new SimilarSchoolsDistanceFilter(currentSchool),
        new SimilarSchoolsUrbanRuralFilter(currentSchool),
    }.ToDictionary(f => f.Key, StringComparer.InvariantCultureIgnoreCase);

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

    public IReadOnlyCollection<SimilarSchoolsAvailableFilter> AsAvailableFilters(IEnumerable<SimilarSchool> items)
    {
        var availableFilters = new List<SimilarSchoolsAvailableFilter>();

        foreach (var (key, filter) in _filters)
        {
            var values = filterValues.ContainsKey(key) ? filterValues[key] : [];

            if (filter is ISimilarSchoolsMultiValueFilter mvf)
            {
                availableFilters.Add(mvf.AsAvailableFilter(items, values));
            }

            if (filter is ISimilarSchoolsSingleValueFilter svf)
            {
                availableFilters.Add(svf.AsAvailableFilter(items, values.LastOrDefault()));
            }
        }

        return availableFilters;
    }
}
