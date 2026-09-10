using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsPersistentAbsenceRateFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsNumericRangeFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => CurrentSchool.PersistentAbsenceRate.Map(v => v.ToString("0.0\\%"));

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, decimal from, decimal to)
    {
        var minValue = DataWithAvailability.Available(from);
        var maxValue = DataWithAvailability.Available(to);

        return items
            .Where(i => minValue <= i.PersistentAbsenceRate && i.PersistentAbsenceRate <= maxValue);
    }
}
