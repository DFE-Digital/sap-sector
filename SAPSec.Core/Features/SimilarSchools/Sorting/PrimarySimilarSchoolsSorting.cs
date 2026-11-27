using SAPSec.Core.Features.Availability;
using SAPSec.Core.Sorting;
using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Core.Features.SimilarSchools.Sorting;

/// <param name="sortBy">The selected sort key; unknown or empty keys fall back to the default option.</param>
/// <param name="includeProgressScores">
/// Adds the KS2 progress score options (behind the EnableKs2ProgressMeasures flag). They are listed first
/// and 'Progress score in reading' becomes the default.
/// </param>
/// <param name="useEarliestProgressYearAsLatest">
/// TESTING ONLY (see Flags.Ks2ProgressUseEarliestYearAsLatest): sort progress scores on the 2022 to 2023 figures.
/// </param>
internal class PrimarySimilarSchoolsSorting(
    string sortBy,
    bool includeProgressScores = false,
    bool useEarliestProgressYearAsLatest = false)
{
    private const string PercentFormat = "0\\%";
    private const int PercentDecimalPlaces = 0;

    private const string ScoreFormat = "0.0";
    private const int ScoreDecimalPlaces = 1;

    private sealed record PrimarySortOption(
        string Key,
        string Name,
        Func<EstablishmentPerformance?, string?> Field,
        string DisplayFormat,
        int DecimalPlaces);

    private IReadOnlyList<PrimarySortOption> Options => [
        .. includeProgressScores ? ProgressOptions() : [],
        new("RwmExpected", "Meeting expected standard in reading, writing and maths", i => i?.RwmExpected_Tot_Cohort_Est_Current_Num, PercentFormat, PercentDecimalPlaces),
        new("RwmHigher", "Achieved a higher standard in reading, writing and maths", i => i?.RwmHigher_Tot_Cohort_Est_Current_Num, PercentFormat, PercentDecimalPlaces),
        new("ReadingScaledScore", "Average scaled score in reading", i => i?.ReadingScaledScore_Tot_Cohort_Est_Current_Num, ScoreFormat, ScoreDecimalPlaces),
        new("MathsScaledScore", "Average scaled score in maths", i => i?.MathsScaledScore_Tot_Cohort_Est_Current_Num, ScoreFormat, ScoreDecimalPlaces),
        new("GpsExpected", "Meeting expected standard in grammar, punctuation and spelling", i => i?.GpsExpected_Tot_Cohort_Est_Current_Num, PercentFormat, PercentDecimalPlaces),
        new("GpsHigher", "Achieved a higher standard in grammar, punctuation and spelling", i => i?.GpsHigher_Tot_Cohort_Est_Current_Num, PercentFormat, PercentDecimalPlaces),
    ];

    private IEnumerable<PrimarySortOption> ProgressOptions() => useEarliestProgressYearAsLatest
        ? [
            new("ReadingProgress", "Progress score in reading", i => i?.ReadingProgress_Tot_Cohort_Est_Previous2_Num, ScoreFormat, ScoreDecimalPlaces),
            new("WritingProgress", "Progress score in writing", i => i?.WritingProgress_Tot_Cohort_Est_Previous2_Num, ScoreFormat, ScoreDecimalPlaces),
            new("MathsProgress", "Progress score in maths", i => i?.MathsProgress_Tot_Cohort_Est_Previous2_Num, ScoreFormat, ScoreDecimalPlaces),
        ]
        : [
            new("ReadingProgress", "Progress score in reading", i => i?.ReadingProgress_Tot_Cohort_Est_Current_Num, ScoreFormat, ScoreDecimalPlaces),
            new("WritingProgress", "Progress score in writing", i => i?.WritingProgress_Tot_Cohort_Est_Current_Num, ScoreFormat, ScoreDecimalPlaces),
            new("MathsProgress", "Progress score in maths", i => i?.MathsProgress_Tot_Cohort_Est_Current_Num, ScoreFormat, ScoreDecimalPlaces),
        ];

    // The first option is the default when the sort key is missing or not recognised.
    private PrimarySortOption Selected(IReadOnlyList<PrimarySortOption> options) =>
        options.FirstOrDefault(o => o.Key.Equals(sortBy, StringComparison.OrdinalIgnoreCase)) ?? options[0];

    public IEnumerable<SortedItem<SimilarSchool, DataWithAvailability<string>>> Sort(
        IEnumerable<SimilarSchoolSortItem<EstablishmentPerformance>> items)
    {
        var selected = Selected(Options);

        return SimilarSchoolsSortEngine.Sort(
            items,
            selected.Key,
            selected.Name,
            i => DataWithAvailability.FromDecimalString(selected.Field(i)),
            selected.DisplayFormat,
            selected.DecimalPlaces);
    }

    public IEnumerable<SortOption> GetPossibleOptions()
    {
        var options = Options;
        var selected = Selected(options);

        return options.Select(o => new SortOption(o.Key, o.Name, o == selected));
    }
}
