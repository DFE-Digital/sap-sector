using SAPSec.Core.DataPoints;
using SAPSec.Core.SimilarSchools;

namespace SAPSec.Core.SimilarSchools.Filtering;

public class SimilarSchoolsPersistentAbsenceRateFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
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
