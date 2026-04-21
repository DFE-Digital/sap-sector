using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsDistanceFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsSingleValueFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string>? CurrentSchoolValue => null;

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return items;
        }

        if (CurrentSchool.Coordinates == null)
        {
            return items;
        }

        return items.Where(i => i.Coordinates is not null &&
            value.ToLowerInvariant() switch
            {
                "5" => i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 5,
                "10" => i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 10,
                "25" => i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 25,
                "50" => i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 50,
                "100" => i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 100,
                _ => true
            });
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, string? value)
    {
        if (CurrentSchool.Coordinates is not null)
        {
            var count = items.Count(i => i.Coordinates is not null && i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 5);
            if (count > 0)
            {
                yield return new FilterOption("5", "Up to 5 miles", count, value == "5");
            }

            count = items.Count(i => i.Coordinates is not null && i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 10);
            if (count > 0)
            {
                yield return new FilterOption("10", "Up to 10 miles", count, value == "10");
            }

            count = items.Count(i => i.Coordinates is not null && i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 25);
            if (count > 0)
            {
                yield return new FilterOption("25", "Up to 25 miles", count, value == "25");
            }

            count = items.Count(i => i.Coordinates is not null && i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 50);
            if (count > 0)
            {
                yield return new FilterOption("50", "Up to 50 miles", count, value == "50");
            }

            count = items.Count(i => i.Coordinates is not null && i.Coordinates.DistanceMiles(CurrentSchool.Coordinates) <= 100);
            if (count > 0)
            {
                yield return new FilterOption("100", "Up to 100 miles", count, value == "100");
            }

            count = items.Count();
            if (count > 0)
            {
                yield return new FilterOption("All", "All schools", count, value?.ToLowerInvariant() == "all");
            }
        }
    }
}
