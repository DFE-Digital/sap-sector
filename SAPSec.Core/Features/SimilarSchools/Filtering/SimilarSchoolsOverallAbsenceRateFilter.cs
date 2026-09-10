using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsOverallAbsenceRateFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsNumericRangeFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => CurrentSchool.OverallAbsenceRate.Map(v => v.ToString("0.0\\%"));

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, decimal from, decimal to)
    {
        var minValue = DataWithAvailability.Available(from);
        var maxValue = DataWithAvailability.Available(to);

        return items
            .Where(i => minValue <= similarSchoolAccessor(i).OverallAbsenceRate && similarSchoolAccessor(i).OverallAbsenceRate <= maxValue);
    }
}
