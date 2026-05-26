using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public record Measure(
    string Key,
    MeasureDataType DataType,
    IReadOnlyCollection<MeasureAvailableFilter> AvailableFilters,
    SchoolKs4ComparisonAverage ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> TopPerformers,
    SchoolKs4ComparisonYearByYear YearByYear)
{
    internal static Measure Build(
        string key,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        var threeYearAverage = Ks4HeadlineMeasuresCalculator.BuildAverage(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolPrevious2(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious2(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandPrevious2(schoolData)));

        var yearByYear = Ks4HeadlineMeasuresCalculator.BuildYearByYear(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.SchoolPrevious2(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious2(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandCurrent(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandPrevious(schoolData)),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(fieldSelector.EnglandPrevious2(schoolData)));

        return new Measure(
            key,
            dataType,
            availableFilters.ToList(),
            BuildComparisonAverage(
                threeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)))),
            BuildTopPerformers(
                schoolData,
                threeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)),
                displayDecimalPlaces: dataType == MeasureDataType.Number ? 1 : 0),
            BuildComparisonYearByYear(
                yearByYear,
                similarSchools.Select(x => SeriesFrom(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)))));
    }

    private static SchoolKs4ComparisonAverage BuildComparisonAverage(
        Ks4HeadlineMeasureAverage current,
        IEnumerable<decimal?> similarSchoolValues) =>
        new(
            current.SchoolValue,
            Average(similarSchoolValues),
            current.LocalAuthorityValue,
            current.EnglandValue);

    private static SchoolKs4ComparisonYearByYear BuildComparisonYearByYear(
        Ks4HeadlineMeasureYearByYear current,
        IEnumerable<Ks4HeadlineMeasureSeries> similarSchoolSeries)
    {
        var similarSeries = similarSchoolSeries.ToArray();

        return new(
            current.School,
            new Ks4HeadlineMeasureSeries(
                Average(similarSeries.Select(x => x.Current)),
                Average(similarSeries.Select(x => x.Previous)),
                Average(similarSeries.Select(x => x.Previous2))),
            current.LocalAuthority,
            current.England);
    }

    private static decimal? MeasureValue(string? current, string? previous, string? previous2) =>
        Ks4HeadlineMeasuresCalculator.Average(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static Ks4HeadlineMeasureSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static decimal? Average(IEnumerable<decimal?> values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<Ks4TopPerformer> BuildTopPerformers(
        SchoolData currentSchool,
        decimal? currentSchoolValue,
        IEnumerable<SchoolData> similarSchoolResponses,
        Func<SchoolData, decimal?> selector,
        int displayDecimalPlaces)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.Urn,
            currentSchool.Name,
            currentSchoolValue,
            IsCurrentSchool: true);

        return similarSchoolResponses
            .Select(response => new TopPerformerCandidate(
                response.Urn,
                response.Name,
                selector(response),
                IsCurrentSchool: false))
            .Append(currentSchoolCandidate)
            .Where(x => x.Value.HasValue)
            .GroupBy(x => x.Urn, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())
            .OrderByDescending(x => TopPerformerSortValue(x.Value, displayDecimalPlaces))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new Ks4TopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();
    }

    private static decimal TopPerformerSortValue(decimal? value, int decimalPlaces) =>
        Math.Round(value!.Value, decimalPlaces, MidpointRounding.AwayFromZero);
}

public enum MeasureDataType
{
    Number,
    Percentage
}

public record MeasureAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options);

public record Ks4TopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false);

public record SchoolKs4ComparisonAverage(
    decimal? SchoolValue,
    decimal? SimilarSchoolsValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record SchoolKs4ComparisonYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries SimilarSchools,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

internal record MeasureFieldSelector(
    Func<SchoolData?, string?> SchoolCurrent,
    Func<SchoolData?, string?> SchoolPrevious,
    Func<SchoolData?, string?> SchoolPrevious2,
    Func<SchoolData?, string?> LocalAuthorityCurrent,
    Func<SchoolData?, string?> LocalAuthorityPrevious,
    Func<SchoolData?, string?> LocalAuthorityPrevious2,
    Func<SchoolData?, string?> EnglandCurrent,
    Func<SchoolData?, string?> EnglandPrevious,
    Func<SchoolData?, string?> EnglandPrevious2);

internal sealed record SchoolData(
    string Urn,
    string Name,
    Ks4PerformanceData? PerformanceData,
    Ks4DestinationsData? DestinationsData);

internal sealed record TopPerformerCandidate(
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool);