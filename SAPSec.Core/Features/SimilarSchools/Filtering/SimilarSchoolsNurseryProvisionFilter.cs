using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsNurseryProvisionFilter(string key,
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
        => DataWithAvailability.FromStringWithoutCodes(CurrentSchool.NurseryProvisionName);

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(similarSchoolAccessor(i).NurseryProvisionName, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values) =>
        items.GroupBy(i => similarSchoolAccessor(i).NurseryProvisionName)
            .Where(f => !string.IsNullOrWhiteSpace(f.Key))
            .Select(g => new FilterOption(
                g.Key,
                g.Key,
                values.Contains(g.Key, StringComparer.OrdinalIgnoreCase),
                g.Count()))
            .OrderBy(fo => fo.Key switch
            {
                "Has Nursery Classes" => 0,
                "No Nursery Classes" => 1,
                _ => 3
            });
}
