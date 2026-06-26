using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsGovernanceStructureFilter(string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsMultiValueFilter(
        key,
        name,
        filterValues,
        currentSchool)
{
    protected override DataWithAvailability<string>? CurrentSchoolValue
        => DataWithAvailability.Available(FindGroup(CurrentSchool).Name);

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i =>
                 (values.Contains("S", StringComparer.OrdinalIgnoreCase) && i.TrustSchoolFlag?.Id == "5") // Single-academy trust (SAT)
              || (values.Contains("M", StringComparer.OrdinalIgnoreCase) && i.TrustSchoolFlag?.Id == "3") // Multi-academy trust (MAT) 
              || (values.Contains("MS", StringComparer.OrdinalIgnoreCase) && (i.TrustSchoolFlag?.Id is "1" or "2" || i.TrustSchoolFlag?.Id == "0" && i.EstablishmentTypeGroup?.Id == "4")) // Maintained school - local authority controlled
              || (values.Contains("N", StringComparer.OrdinalIgnoreCase) && i.TrustSchoolFlag?.Id == "0" && i.EstablishmentTypeGroup?.Id != "4")); // No known group
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        return items
            .GroupBy(FindGroup)
            .Select(g => new FilterOption(
                g.Key!.Key,
                g.Key.Name,
                g.Count(),
                values.Contains(g.Key.Key, StringComparer.OrdinalIgnoreCase)))
            .OrderBy(fo => fo.Key switch
            {
                "S" => 0,
                "M" => 1,
                "MS" => 2,
                _ => 3
            });
    }

    private Group FindGroup(SimilarSchool i)
    {

        if (i.TrustSchoolFlag?.Id == "5")
        {
            return new("S", "Single-academy trust (SAT)");
        }

        if (i.TrustSchoolFlag?.Id == "3")
        {
            return new("M", "Multi-academy trust (MAT)");
        }

        if (i.TrustSchoolFlag?.Id is "1" or "2" || i.TrustSchoolFlag?.Id == "0" && i.EstablishmentTypeGroup?.Id == "4")
        {
            return new("MS", "Maintained school - local authority controlled");
        }

        return new("N", "No known group");
    }

    private record Group(string Key, string Name);
}
