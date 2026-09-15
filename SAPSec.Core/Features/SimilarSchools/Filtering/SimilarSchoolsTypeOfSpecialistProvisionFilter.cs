using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsTypeOfSpecialistProvisionFilter(string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsMultiValueFilter(
        key,
        name,
        filterValues,
        currentSchool)
{
    protected override DataWithAvailability<string>? CurrentSchoolValue
        => DataWithAvailability.Available(FindGroup(CurrentSchool).Name);

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i =>
            (values.Contains("R", StringComparer.OrdinalIgnoreCase)
                && similarSchoolAccessor(i).ResourcedProvision?.Name == "Resourced provision")
            || (values.Contains("RS", StringComparer.OrdinalIgnoreCase)
                && similarSchoolAccessor(i).ResourcedProvision?.Name == "Resourced provision and SEN unit")
            || (values.Contains("S", StringComparer.OrdinalIgnoreCase)
                && similarSchoolAccessor(i).ResourcedProvision?.Name == "SEN unit")
            || (values.Contains("N", StringComparer.OrdinalIgnoreCase)
                && similarSchoolAccessor(i).ResourcedProvision?.Name != "Resourced provision"
                && similarSchoolAccessor(i).ResourcedProvision?.Name != "Resourced provision and SEN unit"
                && similarSchoolAccessor(i).ResourcedProvision?.Name != "SEN unit"));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values)
    {
        return items
            .GroupBy(i => FindGroup(similarSchoolAccessor(i)))
            .Select(g => new FilterOption(
                g.Key!.Key,
                g.Key.Name,
                values.Contains(g.Key.Key, StringComparer.OrdinalIgnoreCase),
                g.Count()))
            .OrderBy(fo => fo.Key switch
            {
                "R" => 0,
                "S" => 1,
                "RS" => 2,
                _ => 3
            });
    }

    private Group FindGroup(SimilarSchool i)
    {
        if (i.ResourcedProvision?.Name == "Resourced provision")
        {
            return new("R", "Resourced provision");
        }

        if (i.ResourcedProvision?.Name == "Resourced provision and SEN unit")
        {
            return new("RS", "Resourced provision and SEN unit");
        }

        if (i.ResourcedProvision?.Name == "SEN unit")
        {
            return new("S", "SEN unit");
        }

        return new("N", "No known specialist provision");
    }

    private record Group(string Key, string Name);
}
