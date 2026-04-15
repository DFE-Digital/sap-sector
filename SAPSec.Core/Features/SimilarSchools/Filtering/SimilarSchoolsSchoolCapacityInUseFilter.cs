using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsSchoolCapacityInUseFilter(SimilarSchool currentSchool) : ISimilarSchoolsNumericRangeFilter
{
    public string Name => "School capacity in use";
    public FilterType Type => FilterType.NumericRange;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? from, string? to)
    {
        var minValue = !string.IsNullOrWhiteSpace(from) && decimal.TryParse(from, out decimal f) ? f : decimal.MinValue;
        var maxValue = !string.IsNullOrWhiteSpace(to) && decimal.TryParse(to, out decimal t) ? t : decimal.MaxValue;

        return items
            .Select(i => new
            {
                Item = i,
                CapacityInUse = CalculateCapacityInUse(i) ?? 0.0M
            })
            .Where(i => minValue <= i.CapacityInUse && i.CapacityInUse <= maxValue)
            .Select(i => i.Item);
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, string? from, string? to) => new(
        key,
        Name,
        Type,
        [],
        CalculateCapacityInUse(currentSchool) is decimal c
            ? DataWithAvailability.Available($"{c}%")
            : DataWithAvailability.NotAvailable<string>());

    private decimal? CalculateCapacityInUse(SimilarSchool school) =>
        school.TotalPupils is not null && school.TotalCapacity is not null
            ? (school.TotalPupils / school.TotalCapacity) * 100.0M
            : null;
}

public class SimilarSchoolsOverallAbsenceRateFilter(SimilarSchool currentSchool) : ISimilarSchoolsNumericRangeFilter
{
    public string Name => "Overall absence rate";
    public FilterType Type => FilterType.NumericRange;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? from, string? to)
    {
        var minValue = !string.IsNullOrWhiteSpace(from) && decimal.TryParse(from, out decimal f) ? DataWithAvailability.Available(f) : DataWithAvailability.NotAvailable<decimal>();
        var maxValue = DataWithAvailability.Available(!string.IsNullOrWhiteSpace(to) && decimal.TryParse(to, out decimal t) ? t : decimal.MaxValue);

        return items
            .Where(i => minValue <= i.PersistentAbsenceRate && i.PersistentAbsenceRate <= maxValue);
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, string? from, string? to) => new(
        key,
        Name,
        Type,
        [],
        CalculateCapacityInUse(currentSchool) is decimal c
            ? DataWithAvailability.Available($"{c}%")
            : DataWithAvailability.NotAvailable<string>());

    private decimal? CalculateCapacityInUse(SimilarSchool school) =>
        school.TotalPupils is not null && school.TotalCapacity is not null
            ? (school.TotalPupils / school.TotalCapacity) * 100.0M
            : null;
}

public class SimilarSchoolsPersistentAbsenceRateFilter(SimilarSchool currentSchool) : ISimilarSchoolsNumericRangeFilter
{
    public string Name => "Persistent absence rate";
    public FilterType Type => FilterType.NumericRange;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? from, string? to)
    {
        var minValue = !string.IsNullOrWhiteSpace(from) && decimal.TryParse(from, out decimal f) ? DataWithAvailability.Available(f) : DataWithAvailability.NotAvailable<decimal>();
        var maxValue = DataWithAvailability.Available(!string.IsNullOrWhiteSpace(to) && decimal.TryParse(to, out decimal t) ? t : decimal.MaxValue);

        return items
            .Where(i => minValue <= i.OverallAbsenceRate && i.OverallAbsenceRate <= maxValue);
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, string? from, string? to) => new(
        key,
        Name,
        Type,
        [],
        CalculateCapacityInUse(currentSchool) is decimal c
            ? DataWithAvailability.Available($"{c}%")
            : DataWithAvailability.NotAvailable<string>());

    private decimal? CalculateCapacityInUse(SimilarSchool school) =>
        school.TotalPupils is not null && school.TotalCapacity is not null
            ? (school.TotalPupils / school.TotalCapacity) * 100.0M
            : null;
}
