using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsDistanceFilter(SimilarSchool currentSchool) : ISimilarSchoolsSingleValueFilter
{
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

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, string? value) => new(
        key,
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
