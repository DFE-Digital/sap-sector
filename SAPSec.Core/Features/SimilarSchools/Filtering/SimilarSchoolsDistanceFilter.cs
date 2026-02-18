using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsDistanceFilter(SimilarSchool currentSchool) : ISimilarSchoolsSingleValueFilter
{
    public string Key => "dist";
    public string Name => "Distance";
    public FilterType Type => FilterType.SingleValue;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return items;
        }

        if (currentSchool.Coordinates == null)
        {
            return items;
        }

        return items.Where(i => i.Coordinates == null ||
            value switch
            {
                "5" => i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 5,
                "10" => i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 10,
                "25" => i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 25,
                "50" => i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 50,
                "100" => i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 100,
                _ => true
            });
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, string? value) => new(
        Key,
        Name,
        Type,
        GetPossibleOptions(items, value).ToList().AsReadOnly(),
        DataWithAvailability.NotApplicable<string>());

    private IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, string? value)
    {
        if (currentSchool.Coordinates == null)
        {
            var count = items.Count();
            yield return new FilterOption("5", "Up to 5 miles", count, value == "5");
            yield return new FilterOption("10", "Up to 10 miles", count, value == "10");
            yield return new FilterOption("25", "Up to 25 miles", count, value == "25");
            yield return new FilterOption("50", "Up to 50 miles", count, value == "50");
            yield return new FilterOption("100", "Up to 100 miles", count, value == "100");
            yield return new FilterOption("Over100", "More than 100 miles", count, value == "Over100");
        }
        else
        {
            var count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 5);
            if (count > 0)
            {
                yield return new FilterOption("5", "Up to 5 miles", count, value == "5");
            }

            count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 10);
            if (count > 0)
            {
                yield return new FilterOption("10", "Up to 10 miles", count, value == "10");
            }

            count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 25);
            if (count > 0)
            {
                yield return new FilterOption("25", "Up to 25 miles", count, value == "25");
            }

            count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 50);
            if (count > 0)
            {
                yield return new FilterOption("50", "Up to 50 miles", count, value == "50");
            }

            count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) <= 100);
            if (count > 0)
            {
                yield return new FilterOption("100", "Up to 100 miles", count, value == "100");
            }

            count = items.Count(i => i.Coordinates == null || i.Coordinates.DistanceMiles(currentSchool.Coordinates) > 100);
            if (count > 0)
            {
                yield return new FilterOption("Over100", "More than 100 miles", count, value == "Over100");
            }
        }
    }
}

public class SimilarSchoolsSchoolCapacityInUseFilter(SimilarSchool currentSchool) : ISimilarSchoolsNumericRangeFilter
{
    public string Key => "sc";
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

    public SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, string? from, string? to) => new(
        Key,
        Name,
        Type,
        [],
        CalculateCapacityInUse(currentSchool) is decimal c
            ? DataWithAvailability.Available($"{c}%")
            : DataWithAvailability.NotAvailable<string>());

    private decimal? CalculateCapacityInUse(SimilarSchool school) =>
        decimal.TryParse(school.TotalPupils, out decimal totalPupils) && decimal.TryParse(school.TotalCapacity, out decimal totalCapacity)
            ? (totalPupils / totalCapacity) * 100.0M
            : null;
}
