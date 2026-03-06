using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsFilters(IDictionary<string, IEnumerable<string>> filterValues, SimilarSchool currentSchool)
{
    private Dictionary<string, ISimilarSchoolsFilter> _filters = new()
    {
        ["dist"] = new SimilarSchoolsDistanceFilter(currentSchool),
        ["reg"] = new SimilarSchoolsRegionFilter(currentSchool),
        ["ur"] = new SimilarSchoolsUrbanRuralFilter(currentSchool),
        ["poe"] = new SimilarSchoolsPhaseOfEducationFilter(currentSchool),
        ["sc"] = new SimilarSchoolsSchoolCapacityInUseFilter(currentSchool),
        ["np"] = new SimilarSchoolsNurseryProvisionFilter(currentSchool),
        ["sf"] = new SimilarSchoolsSixthFormFilter(currentSchool),
        ["ap"] = new SimilarSchoolsAdmissionsPolicyFilter(currentSchool),
        // TODO: Governance structure
        ["sp"] = new SimilarSchoolsTypeOfSpecialistProvisionFilter(currentSchool),
        ["goe"] = new SimilarSchoolsGenderOfEntryFilter(currentSchool),
        // TODO: Overall absence rate
        // TODO: Persistent absence rate
    };

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var filteredItems = items;
        foreach (var (key, filter) in _filters)
        {
            if (filter is ISimilarSchoolsMultiValueFilter mvf)
            {
                var values = filterValues.ContainsKey(key) ? filterValues[key] : [];
                filteredItems = mvf.Filter(filteredItems, values);
            }
            else if (filter is ISimilarSchoolsSingleValueFilter svf)
            {
                var value = (filterValues.ContainsKey(key) ? filterValues[key] : []).LastOrDefault();
                filteredItems = svf.Filter(filteredItems, value);
            }
            else if (filter is ISimilarSchoolsNumericRangeFilter rf)
            {
                var from = (filterValues.ContainsKey(key + "_f") ? filterValues[key + "_f"] : []).LastOrDefault();
                var to = (filterValues.ContainsKey(key + "_t") ? filterValues[key + "_t"] : []).LastOrDefault();
                filteredItems = rf.Filter(filteredItems, from, to);
            }
        }

        return filteredItems;
    }

    public IReadOnlyCollection<SimilarSchoolsAvailableFilter> AsAvailableFilters(IEnumerable<SimilarSchool> items)
    {
        var availableFilters = new List<SimilarSchoolsAvailableFilter>();

        foreach (var (key, filter) in _filters)
        {
            if (filter is ISimilarSchoolsMultiValueFilter mvf)
            {
                var values = filterValues.ContainsKey(key) ? filterValues[key] : [];
                availableFilters.Add(mvf.AsAvailableFilter(key, items, values));
            }
            else if (filter is ISimilarSchoolsSingleValueFilter svf)
            {
                var value = (filterValues.ContainsKey(key) ? filterValues[key] : []).LastOrDefault();
                availableFilters.Add(svf.AsAvailableFilter(key, items, value));
            }
            else if (filter is ISimilarSchoolsNumericRangeFilter rf)
            {
                var from = (filterValues.ContainsKey(key + "_f") ? filterValues[key + "_f"] : []).LastOrDefault();
                var to = (filterValues.ContainsKey(key + "_t") ? filterValues[key + "_t"] : []).LastOrDefault();
                availableFilters.Add(rf.AsAvailableFilter(key, items, from, to));
            }
        }

        return availableFilters;
    }
}
