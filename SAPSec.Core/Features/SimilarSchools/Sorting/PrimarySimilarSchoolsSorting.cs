using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;
using SAPSec.Data.Repositories;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

internal class PrimarySimilarSchoolsSorting(string sortBy)
{
    private const string PercentFormat = "0\\%";
    private const string ScaledScoreFormat = "0.0";

    public IEnumerable<SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>> Sort(
        IEnumerable<PrimaryRankedSimilarSchoolData> items)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "rwmhigher" => Sort(
                items,
                "RwmHigher",
                "Achieved a higher standard in reading, writing and maths",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Current_Num),
                PercentFormat),

            "readingscaledscore" => Sort(
                items,
                "ReadingScaledScore",
                "Average scaled score in reading",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Current_Num),
                ScaledScoreFormat),

            "mathsscaledscore" => Sort(
                items,
                "MathsScaledScore",
                "Average scaled score in maths",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Current_Num),
                ScaledScoreFormat),

            "gpsexpected" => Sort(
                items,
                "GpsExpected",
                "Meeting expected standard in grammar, punctuation and spelling",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Current_Num),
                PercentFormat),

            "gpshigher" => Sort(
                items,
                "GpsHigher",
                "Achieved a higher standard in grammar, punctuation and spelling",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Current_Num),
                PercentFormat),

            _ => Sort(
                items,
                "RwmExpected",
                "Meeting expected standard in reading, writing and maths",
                i => DataWithAvailability.FromDecimalString(i.PerformanceData?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num),
                PercentFormat)
        };
    }

    private static IEnumerable<SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>> Sort(
        IEnumerable<PrimaryRankedSimilarSchoolData> items,
        string sortKey,
        string sortName,
        Func<PrimaryRankedSimilarSchoolData, DataWithAvailability<decimal>> property,
        string displayFormat) =>
        items
            .Select(item => new SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<decimal>>(
                item,
                new SortOptionValue<DataWithAvailability<decimal>>(sortKey, sortName, property(item))))
            .OrderByDescending(i => i.Value.Value, DataWithAvailability<decimal>.Comparer)
            .ThenBy(i => i.Item.SimilarSchool.Name, StringComparer.OrdinalIgnoreCase)
            .Select(item => new SortedItem<PrimaryRankedSimilarSchoolData, DataWithAvailability<string>>(
                item.Item,
                new SortOptionValue<DataWithAvailability<string>>(
                    item.Value.Key,
                    item.Value.Name,
                    item.Value.Value.Map(v => v.ToString(displayFormat)))));

    public IEnumerable<SortOption> GetPossibleOptions(string? selectedSortBy)
    {
        var normalized = selectedSortBy?.ToLowerInvariant() ?? string.Empty;
        var rwmExpectedSelected = normalized is "" or "rwmexpected";

        yield return new("RwmExpected", "Meeting expected standard in reading, writing and maths", rwmExpectedSelected);
        yield return new("RwmHigher", "Achieved a higher standard in reading, writing and maths", normalized == "rwmhigher");
        yield return new("ReadingScaledScore", "Average scaled score in reading", normalized == "readingscaledscore");
        yield return new("MathsScaledScore", "Average scaled score in maths", normalized == "mathsscaledscore");
        yield return new("GpsExpected", "Meeting expected standard in grammar, punctuation and spelling", normalized == "gpsexpected");
        yield return new("GpsHigher", "Achieved a higher standard in grammar, punctuation and spelling", normalized == "gpshigher");
    }
}
