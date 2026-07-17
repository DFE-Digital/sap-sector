using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;
using SAPSec.Data.Repositories;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

internal class PrimarySimilarSchoolsSorting(string sortBy)
{
    public IEnumerable<SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>> Sort(
        IEnumerable<PrimaryRankedSimilarSchoolData> items)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "gpsexpected" => Sort(
                items,
                "GpsExpected",
                "Meeting expected standard in grammar, punctuation and spelling",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Current_Num)),

            _ => Sort(
                items,
                "RwmExpected",
                "Meeting expected standard in reading, writing and maths",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num))
        };
    }

    private static IEnumerable<SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>> Sort(
        IEnumerable<PrimaryRankedSimilarSchoolData> items,
        string sortKey,
        string sortName,
        Func<PrimaryRankedSimilarSchoolData, DataWithAvailability<decimal>> property) =>
        items
            .Select(item => new SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<decimal>>(
                item,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, property(item))))
            .OrderByDescending(i => i.Value.Value, DataWithAvailability<decimal>.Comparer)
            .Select(item => new SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>(
                item.Item,
                new SortOptionValue<DataWithAvailability<string>>(
                    item.Value.Key,
                    item.Value.Name,
                    item.Value.Value.Map(v => v.ToString("0.0\\%")))));

    public IEnumerable<SortOption> GetPossibleOptions(string? selectedSortBy)
    {
        var normalized = selectedSortBy?.ToLowerInvariant() ?? string.Empty;
        var rwmSelected = normalized is "" or "rwmexpected";

        yield return new("RwmExpected", "Meeting expected standard in reading, writing and maths", rwmSelected);
        yield return new("GpsExpected", "Meeting expected standard in grammar, punctuation and spelling", normalized == "gpsexpected");
    }
}
