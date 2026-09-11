using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsReferenceDataFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool,
    Func<SimilarSchool, ReferenceData> selector) : SimilarSchoolsMultiValueFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => DataWithAvailability.FromStringWithCodes(selector(CurrentSchool).Id, selector(CurrentSchool).Name);

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(selector(similarSchoolAccessor(i)).Id, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, IEnumerable<string?> values) =>
        items.GroupBy(i => selector(similarSchoolAccessor(i)))
            .Where(f => !string.IsNullOrWhiteSpace(f.Key.Id) && f.Key.Id != "9")
            .Select(g => new FilterOption(
                g.Key.Id,
                g.Key.Name,
                values.Contains(g.Key.Id, StringComparer.OrdinalIgnoreCase),
                g.Count()))
            .OrderBy(fo => fo.Name);
}
