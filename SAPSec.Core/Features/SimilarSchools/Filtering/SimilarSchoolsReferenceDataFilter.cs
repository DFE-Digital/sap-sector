using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsReferenceDataFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool,
    Func<SimilarSchool, ReferenceData> field) : SimilarSchoolsMultiValueFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => DataWithAvailability.FromStringWithCodes(field(CurrentSchool).Id, field(CurrentSchool).Name);

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(field(similarSchoolAccessor(i)).Id, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values) =>
        items.GroupBy(i => field(similarSchoolAccessor(i)))
            .Where(f => !string.IsNullOrWhiteSpace(f.Key.Id) && f.Key.Id != "9")
            .Select(g => new FilterOption(
                g.Key.Id,
                g.Key.Name,
                values.Contains(g.Key.Id, StringComparer.OrdinalIgnoreCase),
                g.Count()))
            .OrderBy(fo => fo.Name);
}
