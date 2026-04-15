using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsFilters(IDictionary<string, IEnumerable<string>> filterValues, SimilarSchool currentSchool)
{
    private readonly Dictionary<string, IEnumerable<string>> _filterValues = filterValues
        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, ISimilarSchoolsFilter> _filters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dist"] = new SimilarSchoolsDistanceFilter(currentSchool),
        ["reg"] = new SimilarSchoolsRegionFilter(currentSchool),
        ["ur"] = new SimilarSchoolsUrbanRuralFilter(currentSchool),
        ["poe"] = new SimilarSchoolsPhaseOfEducationFilter(currentSchool),
        ["sciu"] = new SimilarSchoolsSchoolCapacityInUseFilter(currentSchool),
        ["np"] = new SimilarSchoolsNurseryProvisionFilter(currentSchool),
        ["sf"] = new SimilarSchoolsSixthFormFilter(currentSchool),
        ["ap"] = new SimilarSchoolsAdmissionsPolicyFilter(currentSchool),
        // TODO: Governance structure
        ["sp"] = new SimilarSchoolsTypeOfSpecialistProvisionFilter(currentSchool),
        ["goe"] = new SimilarSchoolsGenderOfEntryFilter(currentSchool),
        ["oar"] = new SimilarSchoolsOverallAbsenceRateFilter(currentSchool),
        ["par"] = new SimilarSchoolsPersistentAbsenceRateFilter(currentSchool)
    };

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var filteredItems = items;
        foreach (var (key, filter) in _filters)
        {
            if (filter is ISimilarSchoolsMultiValueFilter mvf)
            {
                var values = _filterValues.ContainsKey(key) ? _filterValues[key] : [];
                filteredItems = mvf.Filter(filteredItems, values);
            }
            else if (filter is ISimilarSchoolsSingleValueFilter svf)
            {
                var value = (_filterValues.ContainsKey(key) ? _filterValues[key] : []).LastOrDefault();
                filteredItems = svf.Filter(filteredItems, value);
            }
            else if (filter is ISimilarSchoolsNumericRangeFilter rf)
            {
                var from = (_filterValues.ContainsKey(key + "_f") ? _filterValues[key + "_f"] : []).LastOrDefault();
                var to = (_filterValues.ContainsKey(key + "_t") ? _filterValues[key + "_t"] : []).LastOrDefault();
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
                var values = _filterValues.ContainsKey(key) ? _filterValues[key] : [];
                var availableFilter = mvf.AsAvailableFilter(key, items, values);
                if (availableFilter.Options.Count > 1)
                {
                    availableFilters.Add(availableFilter);
                }
            }
            else if (filter is ISimilarSchoolsSingleValueFilter svf)
            {
                var value = (_filterValues.ContainsKey(key) ? _filterValues[key] : []).LastOrDefault();
                var availableFilter = svf.AsAvailableFilter(key, items, value);
                if (availableFilter.Options.Count > 1)
                {
                    availableFilters.Add(availableFilter);
                }
            }
            else if (filter is ISimilarSchoolsNumericRangeFilter rf)
            {
                var from = (_filterValues.ContainsKey(key + "_f") ? _filterValues[key + "_f"] : []).LastOrDefault();
                var to = (_filterValues.ContainsKey(key + "_t") ? _filterValues[key + "_t"] : []).LastOrDefault();
                var availableFilter = rf.AsAvailableFilter(key, items, from, to);
                availableFilters.Add(availableFilter);
            }
        }

        return availableFilters;
    }
}
