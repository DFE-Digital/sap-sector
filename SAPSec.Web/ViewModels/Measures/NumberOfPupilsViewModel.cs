using System.Globalization;
using SAPSec.Core.Features.Measures.Primary;

namespace SAPSec.Web.ViewModels.Measures;

/// <summary>
/// The 'Number of pupils' details component shown under each view of a KS2 progress measure.
/// </summary>
public record NumberOfPupilsViewModel(
    string Id,
    IReadOnlyList<string> YearLabels,
    IReadOnlyList<NumberOfPupilsSchoolViewModel> Schools)
{
    public const string NoData = "No available data";
    public const string NotApplicable = "N/A";

    /// <summary>One column: the latest year (bar chart and top performers).</summary>
    public static NumberOfPupilsViewModel ForLatestYear(
        string id,
        int year,
        IEnumerable<(string Name, SchoolPupilNumbers? Numbers)> schools) =>
        new(
            id,
            [AcademicYearLabelConfig.FormatAcademicYear(year)],
            schools.Select(s => NumberOfPupilsSchoolViewModel.From(s.Name, s.Numbers, latestYearOnly: true)).ToList());

    /// <summary>Three columns: latest year - 2, latest year - 1, latest year (year by year and table).</summary>
    public static NumberOfPupilsViewModel ForYearByYear(
        string id,
        int year,
        IEnumerable<(string Name, SchoolPupilNumbers? Numbers)> schools) =>
        new(
            id,
            AcademicYearLabelConfig.FormatYearByYear(year),
            schools.Select(s => NumberOfPupilsSchoolViewModel.From(s.Name, s.Numbers, latestYearOnly: false)).ToList());
}

public record NumberOfPupilsSchoolViewModel(
    string Name,
    IReadOnlyList<string> EligiblePupils,
    IReadOnlyList<string> ProportionIncluded)
{
    public static NumberOfPupilsSchoolViewModel From(string name, SchoolPupilNumbers? numbers, bool latestYearOnly)
    {
        int?[] eligible = numbers is null
            ? [null, null, null]
            : [numbers.EligiblePupils.Previous2, numbers.EligiblePupils.Previous, numbers.EligiblePupils.Current];

        // A null proportion set means it isn't published for the pupil characteristic, so it shows N/A
        // rather than 'No available data'.
        string[] proportion = numbers is null
            ? [NumberOfPupilsViewModel.NoData, NumberOfPupilsViewModel.NoData, NumberOfPupilsViewModel.NoData]
            : numbers.ProportionIncluded is null
                ? [NumberOfPupilsViewModel.NotApplicable, NumberOfPupilsViewModel.NotApplicable, NumberOfPupilsViewModel.NotApplicable]
                : [
                    DisplayPercent(numbers.ProportionIncluded.Previous2),
                    DisplayPercent(numbers.ProportionIncluded.Previous),
                    DisplayPercent(numbers.ProportionIncluded.Current)
                ];

        var eligibleText = eligible.Select(DisplayCount).ToArray();

        return latestYearOnly
            ? new(name, [eligibleText[^1]], [proportion[^1]])
            : new(name, eligibleText, proportion);
    }

    private static string DisplayCount(int? value) =>
        value.HasValue
            ? value.Value.ToString("N0", CultureInfo.InvariantCulture)
            : NumberOfPupilsViewModel.NoData;

    // Proportion of pupils is shown as a percentage to 0 decimal places.
    private static string DisplayPercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : NumberOfPupilsViewModel.NoData;
}

/// <summary>
/// The 'Number of pupils' tables for each view of a measure. Top performers is null on comparison pages.
/// </summary>
public record MeasureNumberOfPupilsViewModel(
    NumberOfPupilsViewModel CurrentYear,
    NumberOfPupilsViewModel YearByYear,
    NumberOfPupilsViewModel Table,
    NumberOfPupilsViewModel? TopPerformers)
{
    /// <param name="htmlPrefix">The measure's HTML prefix, used to give each details component a unique id.</param>
    /// <param name="year">The measure's latest year.</param>
    /// <param name="schools">The school(s) being viewed: the user's school, plus the similar school on comparison pages.</param>
    /// <param name="topPerformers">The top performing schools in rank order, or null when the view has no top performers.</param>
    public static MeasureNumberOfPupilsViewModel Create(
        string htmlPrefix,
        int year,
        IReadOnlyList<(string Name, SchoolPupilNumbers? Numbers)> schools,
        IReadOnlyList<(string Name, SchoolPupilNumbers? Numbers)>? topPerformers) =>
        new(
            NumberOfPupilsViewModel.ForLatestYear($"{htmlPrefix}-current-year-number-of-pupils", year, schools),
            NumberOfPupilsViewModel.ForYearByYear($"{htmlPrefix}-year-by-year-number-of-pupils", year, schools),
            NumberOfPupilsViewModel.ForYearByYear($"{htmlPrefix}-table-number-of-pupils", year, schools),
            topPerformers is null
                ? null
                : NumberOfPupilsViewModel.ForLatestYear($"{htmlPrefix}-top-performers-number-of-pupils", year, topPerformers));
}
