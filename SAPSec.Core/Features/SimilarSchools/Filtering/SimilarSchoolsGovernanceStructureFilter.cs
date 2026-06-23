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
        var temp = items.Where(i =>
                 (values.Contains("S", StringComparer.OrdinalIgnoreCase) && i.TrustSchoolFlag?.Id == "5")
              || (values.Contains("M", StringComparer.OrdinalIgnoreCase) && i.TrustSchoolFlag?.Id == "3")
              || (values.Contains("MS", StringComparer.OrdinalIgnoreCase) && (i.TrustSchoolFlag?.Id == "1" || i.TrustSchoolFlag?.Id == "2"))
              || (values.Contains("MS", StringComparer.OrdinalIgnoreCase) && (i.TrustSchoolFlag?.Id == "0" || i.EstablishmentTypeGroup?.Id == "4"))
              || (values.Contains("N", StringComparer.OrdinalIgnoreCase) && ((i.TrustSchoolFlag?.Id == "0" || i.TrustSchoolFlag?.Id == "") && (i.EstablishmentTypeGroup?.Id == "0" || i.EstablishmentTypeGroup?.Id == ""))));

        var count = temp.Count();

        return temp;
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

        if (i.TrustSchoolFlag?.Id == "1" || i.TrustSchoolFlag?.Id == "2")
        {
            return new("MS", "Maintained scool - local authority controlled");
        }

        if (i.TrustSchoolFlag?.Id == "0" && i.EstablishmentTypeGroup?.Id == "4")
        {
            return new("MS", "Maintained scool - local authority controlled");
        }

        if ((i.TrustSchoolFlag?.Id == "0" || i.TrustSchoolFlag?.Id == "") && (i.EstablishmentTypeGroup?.Id == "0" || i.EstablishmentTypeGroup?.Id == ""))
        {
            return new("N", "No known group");
        }

        return new("N", "No known group");
    }

    private record Group(string Key, string Name);
}
