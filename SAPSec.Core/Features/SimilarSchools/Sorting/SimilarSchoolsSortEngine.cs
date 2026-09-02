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
    public static IEnumerable<SortedItem<TItem, DataWithAvailability<string>>> Sort<TItem>(
        IEnumerable<TItem> items,
        string sortKey,
        string sortName,
        Func<TItem, DataWithAvailability<decimal>> valueSelector,
        Func<TItem, string> nameSelector,
        string displayFormat,
        int decimalPlaces) =>
        items
            .Select(item => new SortedItem<TItem, DataWithAvailability<decimal>>(
                item,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, valueSelector(item))))
            // Sort on the value rounded to the precision shown to the user, so schools that
            // display the same score (e.g. both "83%") are treated as tied.
            .OrderByDescending(
                i => i.Value.Value.Map(v => Math.Round(v, decimalPlaces, MidpointRounding.AwayFromZero)),
                DataWithAvailability<decimal>.Comparer)
            .ThenBy(i => nameSelector(i.Item), StringComparer.OrdinalIgnoreCase)
            .Select(item => new SortedItem<TItem, DataWithAvailability<string>>(
                item.Item,
                new SortOptionValue<DataWithAvailability<string>>(
                    item.Value.Key,
                    item.Value.Name,
                    item.Value.Value.Map(v => v.ToString(displayFormat)))));
}
