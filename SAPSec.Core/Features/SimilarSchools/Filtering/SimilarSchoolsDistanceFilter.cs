using SAPSec.Core.Collections;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsDistanceFilter(
    string key,
    string name,
    CaseInsensitiveDictionary<IEnumerable<string>> filterValues,
    SimilarSchool currentSchool) : SimilarSchoolsSingleValueFilter(key, name, filterValues, currentSchool)
{
    protected override DataWithAvailability<string>? CurrentSchoolValue => null;

    protected override IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return items;
        }

        if (CurrentSchool.Coordinates == null)
        {
            return items;
        }

        var filtered = items.Where(i => similarSchoolAccessor(i).Coordinates is not null &&
            value.ToLowerInvariant() switch
            {
                "5" => similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 5,
                "10" => similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 10,
                "25" => similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 25,
                "50" => similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 50,
                "100" => similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 100,
                _ => true
            }).ToList();

        var missing = items.Except(filtered).ToList();
        return filtered;
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions<T>(IEnumerable<T> items, Func<T, SimilarSchool> similarSchoolAccessor, string? value)
    {
        if (CurrentSchool.Coordinates is not null)
        {
            var count = items.Count(i => similarSchoolAccessor(i).Coordinates is not null && similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 5);
            if (count > 0)
            {
                yield return new FilterOption("5", "Up to 5 miles", value == "5", count);
            }

            count = items.Count(i => similarSchoolAccessor(i).Coordinates is not null && similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 10);
            if (count > 0)
            {
                yield return new FilterOption("10", "Up to 10 miles", value == "10", count);
            }

            count = items.Count(i => similarSchoolAccessor(i).Coordinates is not null && similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 25);
            if (count > 0)
            {
                yield return new FilterOption("25", "Up to 25 miles", value == "25", count);
            }

            count = items.Count(i => similarSchoolAccessor(i).Coordinates is not null && similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 50);
            if (count > 0)
            {
                yield return new FilterOption("50", "Up to 50 miles", value == "50", count);
            }

            count = items.Count(i => similarSchoolAccessor(i).Coordinates is not null && similarSchoolAccessor(i).Coordinates?.DistanceMiles(CurrentSchool.Coordinates) <= 100);
            if (count > 0)
            {
                yield return new FilterOption("100", "Up to 100 miles", value == "100", count);
            }

            count = items.Count();
            if (count > 0)
            {
                yield return new FilterOption("All", "All schools", value?.ToLowerInvariant() == "all", count);
            }
        }
    }
}
