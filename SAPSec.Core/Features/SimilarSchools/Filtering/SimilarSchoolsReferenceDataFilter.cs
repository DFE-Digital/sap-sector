using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsReferenceDataFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool,
    Func<SimilarSchool, ReferenceData> field) : SimilarSchoolsMultiValueFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => DataWithAvailability.FromStringWithCodes(field(CurrentSchool).Id, field(CurrentSchool).Name);

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(field(i).Id, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(field)
            .Where(f => !string.IsNullOrWhiteSpace(f.Key.Id) && f.Key.Id != "9")
            .Select(g => new FilterOption(
                g.Key.Id,
                g.Key.Name,
                g.Count(),
                values.Contains(g.Key.Id, StringComparer.OrdinalIgnoreCase)))
            .OrderBy(fo => fo.Name);
}
