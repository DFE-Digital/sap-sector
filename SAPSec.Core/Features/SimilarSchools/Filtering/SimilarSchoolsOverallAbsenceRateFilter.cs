using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsOverallAbsenceRateFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsNumericRangeFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => CurrentSchool.OverallAbsenceRate.Map(v => v.ToString("0.0\\%"));

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, decimal from, decimal to)
    {
        var minValue = DataWithAvailability.Available(from);
        var maxValue = DataWithAvailability.Available(to);

        return items
            .Where(i => minValue <= i.OverallAbsenceRate && i.OverallAbsenceRate <= maxValue);
    }
}
