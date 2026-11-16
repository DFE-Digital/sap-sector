using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.ViewModels.ChartExamples;

/// <summary>
/// Made-up KS2 progress scores covering the scenarios we need product and design to review:
/// all positive, all negative, mixed, wide ranges and the expected extremes (-25 to 30).
/// None of these are real school results.
/// </summary>
public record Ks2ProgressChartExamplesViewModel(IReadOnlyList<ChartExampleViewModel> Examples)
{
    private const int Year = 2024;

    private static readonly SchoolInfo ExampleSchool = new(
        "000000",
        "Example Primary School",
        new LocalAuthority("000", "Example local authority"),
        new Address("", "", "", "", ""));

    public static Ks2ProgressChartExamplesViewModel Create() => new([
        Example("all-positive", "All scores positive",
            "Should look like the other KS2 measures: bars start at 0.",
            new(2.3m, 1.9m, 1.2m), new(1.1m, 1.0m, 0.8m), new(0.4m, 0.3m, 0.5m), new(0.0m, 0.0m, 0.0m)),

        Example("all-negative", "All scores negative",
            "Bars go left from 0, which sits on the right-hand side of the chart.",
            new(-2.5m, -1.4m, -1.9m), new(-1.1m, -0.9m, -1.2m), new(-0.5m, -0.4m, -0.6m), new(0.0m, 0.0m, 0.0m)),

        Example("mixed", "Positive and negative scores",
            "A diverging bar chart with 0 as the central baseline.",
            new(-1.8m, 0.6m, 1.1m), new(1.1m, 0.9m, 0.7m), new(0.3m, -0.2m, 0.1m), new(0.0m, 0.0m, 0.0m)),

        Example("wide-range", "Wide range: -21.2, -1.8, 1.1, 2.3",
            "The example from the ticket. The baseline moves right to make room for the outlier.",
            new(-21.2m, -15.4m, -9.8m), new(-1.8m, -1.2m, -1.5m), new(1.1m, 0.8m, 0.9m), new(2.3m, 1.9m, 2.0m)),

        Example("extreme-negative", "Extreme negative score: -25",
            "The lowest score we expect. Scores near 0 become short bars.",
            new(-25.0m, -12.3m, -4.1m), new(-1.2m, -0.8m, -1.0m), new(0.3m, 0.2m, 0.4m), new(0.0m, 0.0m, 0.0m)),

        Example("extreme-positive", "Extreme positive score: 30",
            "The highest score we expect.",
            new(30.0m, 18.6m, 6.2m), new(1.1m, 0.9m, 1.3m), new(0.4m, 0.6m, 0.5m), new(0.0m, 0.0m, 0.0m)),

        Example("extreme-both", "Both extremes: -25 and 30",
            "The full expected range on one chart: -30 to 30.",
            new(-25.0m, -3.1m, 2.4m), new(30.0m, 12.5m, 4.2m), new(0.5m, 0.4m, 0.3m), new(0.0m, 0.0m, 0.0m)),

        Example("missing-data", "School has no score this year",
            "How a missing value shows alongside negative scores.",
            new(null, -0.6m, -1.4m), new(0.5m, 0.4m, 0.6m), new(-0.3m, -0.1m, -0.2m), new(0.0m, 0.0m, 0.0m)),
    ]);

    private static ChartExampleViewModel Example(
        string key,
        string title,
        string description,
        Scores school,
        Scores similarSchools,
        Scores localAuthority,
        Scores england)
    {
        var measure = new Measure(
            $"ks2-progress-example-{key}",
            title,
            Year,
            MeasureDataType.ProgressScore,
            [],
            [
                new MeasureSeries(MeasureSeriesType.CurrentSchool, school.Current, school.Previous, school.Previous2),
                new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, similarSchools.Current, similarSchools.Previous, similarSchools.Previous2),
                new MeasureSeries(MeasureSeriesType.LASchoolsAverage, localAuthority.Current, localAuthority.Previous, localAuthority.Previous2),
                new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, england.Current, england.Previous, england.Previous2),
            ]);

        return new ChartExampleViewModel(
            key,
            title,
            description,
            MeasureViewModel.FromPrimaryMeasure(measure, ExampleSchool, hasSimilarSchools: false));
    }

    /// <summary>Scores for one series, newest year first.</summary>
    private sealed record Scores(decimal? Current, decimal? Previous, decimal? Previous2);
}

public record ChartExampleViewModel(
    string Key,
    string Title,
    string Description,
    MeasureViewModel Measure);
