using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsNurseryProvisionFilter(string key,
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
        => DataWithAvailability.FromStringWithoutCodes(CurrentSchool.NurseryProvisionName);

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(i.NurseryProvisionName, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(i => i.NurseryProvisionName)
            .Where(f => !string.IsNullOrWhiteSpace(f.Key))
            .Select(g => new FilterOption(
                g.Key,
                g.Key,
                g.Count(),
                values.Contains(g.Key, StringComparer.OrdinalIgnoreCase)))
            .OrderBy(fo => fo.Key switch
            {
                "Has Nursery Classes" => 0,
                "No Nursery Classes" => 1,
                _ => 3
            });
}
