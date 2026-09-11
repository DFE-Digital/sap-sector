using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Sorting;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

/// <summary>
/// Shared sort mechanic used by both primary and secondary similar schools sorting:
/// order by value descending, then break ties alphabetically by school name.
/// Sortable metrics differ per phase (GCSE grades vs KS2 measures), so the available
/// sort keys stay defined separately in SimilarSchoolsSorting/PrimarySimilarSchoolsSorting -
/// this only holds the ordering logic they both need.
/// </summary>
internal static class SimilarSchoolsSortEngine
{
    public static IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort<TSortData>(
        IEnumerable<SimilarSchoolSortItem<TSortData>> items,
        string sortKey,
        string sortName,
        Func<TSortData?, DataWithAvailability<decimal>> valueSelector,
        string displayFormat,
        int decimalPlaces)
            where TSortData : class =>
        items
            .Select(item => new SortedItem<SimilarSchool, DataWithAvailability<decimal>>(
                item.SimilarSchool,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, valueSelector(item.SortData))))
            // Sort on the value rounded to the precision shown to the user, so schools that
            // display the same score (e.g. both "83%") are treated as tied.
            .OrderByDescending(
                i => i.Value.Value.Map(v => Math.Round(v, decimalPlaces, MidpointRounding.AwayFromZero)),
                DataWithAvailability<decimal>.Comparer)
            .ThenBy(i => i.Item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(item => new SortedItem<SimilarSchool, DataWithAvailability<string>>(
                item.Item,
                new SortOptionValue<DataWithAvailability<string>>(
                    item.Value.Key,
                    item.Value.Name,
                    item.Value.Value.Map(v => v.ToString(displayFormat)))));
}
