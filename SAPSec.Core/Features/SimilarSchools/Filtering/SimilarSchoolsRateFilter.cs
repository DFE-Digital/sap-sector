using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsRateFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool,
    Func<SimilarSchool, DataWithAvailability<decimal>> rateAccessor,
    string format) : SimilarSchoolsNumericRangeFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => rateAccessor(CurrentSchool).Map(v => v.ToString(format));

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, decimal from, decimal to)
    {
        var minValue = DataWithAvailability.Available(from);
        var maxValue = DataWithAvailability.Available(to);

        return items
            .Where(i => minValue <= rateAccessor(similarSchoolAccessor(i)) && rateAccessor(similarSchoolAccessor(i)) <= maxValue);
    }
}
