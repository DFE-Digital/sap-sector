using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsSchoolCapacityInUseFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsNumericRangeFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string> CurrentSchoolValue
        => CalculateCapacityInUse(CurrentSchool) is decimal c
            ? DataWithAvailability.Available(c.ToString("0.0\\%"))
            : DataWithAvailability.NotAvailable<string>();

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, decimal from, decimal to)
    {
        return items
            .Select(i => new
            {
                Item = i,
                CapacityInUse = CalculateCapacityInUse(i)
            })
            .Where(i => i.CapacityInUse is not null && from <= i.CapacityInUse && i.CapacityInUse <= to)
            .Select(i => i.Item);
    }

    private decimal? CalculateCapacityInUse(SimilarSchool school) =>
        school.TotalPupils is not null && school.TotalCapacity is not null
            ? ((decimal)school.TotalPupils / (decimal)school.TotalCapacity) * 100.0M
            : null;
}
