using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsFilters(IDictionary<string, IEnumerable<string>> filterValues, SimilarSchool currentSchool)
{
    private Dictionary<string, ISimilarSchoolsFilter> _filters = new ISimilarSchoolsFilter[]
    {
        new SimilarSchoolsDistanceFilter("dist", "Distance", filterValues, currentSchool),
        new SimilarSchoolsReferenceDataFilter("reg", "Region", filterValues, currentSchool, s => s.Region),
        new SimilarSchoolsReferenceDataFilter("ur", "Urban or rural", filterValues, currentSchool, s => s.UrbanRural),
        new SimilarSchoolsReferenceDataFilter("poe", "Phase of education", filterValues, currentSchool, s => s.PhaseOfEducation),
        new SimilarSchoolsSchoolCapacityInUseFilter("sciu", "School capacity in use", filterValues, currentSchool),
        new SimilarSchoolsNurseryProvisionFilter("np", "Nursery provision", filterValues, currentSchool),
        new SimilarSchoolsReferenceDataFilter("sf", "Sixth form", filterValues, currentSchool, s => s.OfficialSixthForm),
        new SimilarSchoolsReferenceDataFilter("ap", "Admissions policy", filterValues, currentSchool, s => s.AdmissionsPolicy),
        // TODO: Governance structure
        new SimilarSchoolsTypeOfSpecialistProvisionFilter("sp", "Type of specialist provision", filterValues, currentSchool),
        new SimilarSchoolsReferenceDataFilter("goe", "Gender of entry", filterValues, currentSchool, s => s.Gender),
        new SimilarSchoolsOverallAbsenceRateFilter("oar", "Overall absence rate", filterValues, currentSchool),
        new SimilarSchoolsPersistentAbsenceRateFilter("par", "Persistent absence rate", filterValues, currentSchool)
    }.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ValidationError> Validate()
    {
        var errors = new List<ValidationError>();

        foreach (var filter in _filters.Values.OfType<SimilarSchoolsNumericRangeFilter>())
        {
            if (filter.IsApplied)
            {
                errors.AddRange(filter.Validate());
            }
        }

        return errors;
    }

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        var filteredItems = items;

        foreach (var filter in _filters.Values)
        {
            if (filter.IsApplied)
            {
                filteredItems = filter.Filter(filteredItems);
            }
        }

        return filteredItems;
    }

    public IReadOnlyCollection<SimilarSchoolsAvailableFilter> AsAvailableFilters(IEnumerable<SimilarSchool> items)
    {
        var availableFilters = new List<SimilarSchoolsAvailableFilter>();

        foreach (var (key, filter) in _filters)
        {
            var availableFilter = filter.AsAvailableFilter(items);
            if (availableFilter is not null)
            {
                availableFilters.Add(availableFilter);
            }
        }

        return availableFilters;
    }
}
