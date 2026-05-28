using SAPSec.Core.Features.Filtering;
using System.Globalization;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public record SchoolMeasure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IReadOnlyCollection<MeasureAvailableFilter> AvailableFilters,
    SchoolMeasureThreeYearAverage ThreeYearAverage,
    IReadOnlyList<MeasureTopPerformer> TopPerformers,
    SchoolMeasureYearByYear YearByYear)
{
    internal static SchoolMeasure Build(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        return new SchoolMeasure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            BuildThreeYearAverage(schoolData, similarSchools, fieldSelector),
            BuildTopPerformers(schoolData, similarSchools, fieldSelector),
            BuildYearByYear(schoolData, similarSchools, fieldSelector));
    }

    private static SchoolMeasureThreeYearAverage BuildThreeYearAverage(
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector) => new SchoolMeasureThreeYearAverage(
            AverageFrom(
                fieldSelector.SchoolCurrent(schoolData),
                fieldSelector.SchoolPrevious(schoolData),
                fieldSelector.SchoolPrevious2(schoolData)),
            Average(similarSchools.Select(x => AverageFrom(
                fieldSelector.SchoolCurrent(x),
                fieldSelector.SchoolPrevious(x),
                fieldSelector.SchoolPrevious2(x)))),
            AverageFrom(
                fieldSelector.LocalAuthorityCurrent(schoolData),
                fieldSelector.LocalAuthorityPrevious(schoolData),
                fieldSelector.LocalAuthorityPrevious2(schoolData)),
            AverageFrom(
                fieldSelector.EnglandCurrent(schoolData),
                fieldSelector.EnglandPrevious(schoolData),
                fieldSelector.EnglandPrevious2(schoolData)));

    private static SchoolMeasureYearByYear BuildYearByYear(
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector) => new SchoolMeasureYearByYear(
            SeriesFrom(
                fieldSelector.SchoolCurrent(schoolData),
                fieldSelector.SchoolPrevious(schoolData),
                fieldSelector.SchoolPrevious2(schoolData)),
            new MeasureYearByYearSeries(
                AverageFrom(similarSchools.Select(x => fieldSelector.SchoolCurrent(x))),
                AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious(x))),
                AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x)))),
            SeriesFrom(
                fieldSelector.LocalAuthorityCurrent(schoolData),
                fieldSelector.LocalAuthorityPrevious(schoolData),
                fieldSelector.LocalAuthorityPrevious2(schoolData)),
            SeriesFrom(
                fieldSelector.EnglandCurrent(schoolData),
                fieldSelector.EnglandPrevious(schoolData),
                fieldSelector.EnglandPrevious2(schoolData)));

    private static IReadOnlyList<MeasureTopPerformer> BuildTopPerformers(
        SchoolData currentSchool,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.Urn,
            currentSchool.Name,
            AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool),
                fieldSelector.SchoolPrevious(currentSchool),
                fieldSelector.SchoolPrevious2(currentSchool)),
            IsCurrentSchool: true);

        return similarSchools
            .Select(x => new TopPerformerCandidate(
                x.Urn,
                x.Name,
                AverageFrom(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)),
                IsCurrentSchool: false))
            .Append(currentSchoolCandidate)
            .Where(x => x.Value.HasValue)
            .GroupBy(x => x.Urn, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new MeasureTopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();
    }


    private static decimal? AverageFrom(IEnumerable<string?> stringValues) =>
        Average(stringValues.Select(ParseNullableDecimal));

    internal static decimal? AverageFrom(params string?[] values) => AverageFrom((IEnumerable<string?>)values);

    private static MeasureYearByYearSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            ParseNullableDecimal(current),
            ParseNullableDecimal(previous),
            ParseNullableDecimal(previous2));

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

    internal static decimal? Average(params decimal?[] values) => Average((IEnumerable<decimal?>)values);

    internal static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}

public record SchoolComparisonMeasure(
    string Key,
    string Name,
    MeasureDataType DataType,
    IReadOnlyCollection<MeasureAvailableFilter> AvailableFilters,
    SchoolComparisonMeasureThreeYearAverage ThreeYearAverage,
    IReadOnlyList<MeasureTopPerformer> TopPerformers,
    SchoolComparisonMeasureYearByYear YearByYear)
{
    internal static SchoolComparisonMeasure Build(
        string key,
        string name,
        MeasureDataType dataType,
        IEnumerable<MeasureAvailableFilter> availableFilters,
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        return new SchoolComparisonMeasure(
            key,
            name,
            dataType,
            availableFilters.ToList(),
            BuildThreeYearAverage(currentSchoolData, similarSchoolData, fieldSelector),
            BuildTopPerformers(currentSchoolData, similarSchools, fieldSelector),
            BuildYearByYear(currentSchoolData, similarSchoolData, fieldSelector));
    }

    private static SchoolComparisonMeasureThreeYearAverage BuildThreeYearAverage(
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        MeasureFieldSelector fieldSelector) => new SchoolComparisonMeasureThreeYearAverage(
            AverageFrom(
                fieldSelector.SchoolCurrent(currentSchoolData),
                fieldSelector.SchoolPrevious(currentSchoolData),
                fieldSelector.SchoolPrevious2(currentSchoolData)),
            AverageFrom(
                fieldSelector.SchoolCurrent(similarSchoolData),
                fieldSelector.SchoolPrevious(similarSchoolData),
                fieldSelector.SchoolPrevious2(similarSchoolData)),
            AverageFrom(
                fieldSelector.EnglandCurrent(currentSchoolData),
                fieldSelector.EnglandPrevious(currentSchoolData),
                fieldSelector.EnglandPrevious2(currentSchoolData)));

    private static SchoolComparisonMeasureYearByYear BuildYearByYear(
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        MeasureFieldSelector fieldSelector) => new SchoolComparisonMeasureYearByYear(
            SeriesFrom(
                fieldSelector.SchoolCurrent(currentSchoolData),
                fieldSelector.SchoolPrevious(currentSchoolData),
                fieldSelector.SchoolPrevious2(currentSchoolData)),
            SeriesFrom(
                fieldSelector.SchoolCurrent(similarSchoolData),
                fieldSelector.SchoolPrevious(similarSchoolData),
                fieldSelector.SchoolPrevious2(similarSchoolData)),
            SeriesFrom(
                fieldSelector.EnglandCurrent(currentSchoolData),
                fieldSelector.EnglandPrevious(currentSchoolData),
                fieldSelector.EnglandPrevious2(currentSchoolData)));

    private static IReadOnlyList<MeasureTopPerformer> BuildTopPerformers(
        SchoolData currentSchool,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.Urn,
            currentSchool.Name,
            AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool),
                fieldSelector.SchoolPrevious(currentSchool),
                fieldSelector.SchoolPrevious2(currentSchool)),
            IsCurrentSchool: true);

        return similarSchools
            .Select(x => new TopPerformerCandidate(
                x.Urn,
                x.Name,
                AverageFrom(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)),
                IsCurrentSchool: false))
            .Append(currentSchoolCandidate)
            .Where(x => x.Value.HasValue)
            .GroupBy(x => x.Urn, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new MeasureTopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();
    }

    private static decimal? AverageFrom(IEnumerable<string?> stringValues) =>
        Average(stringValues.Select(ParseNullableDecimal));

    internal static decimal? AverageFrom(params string?[] values) => AverageFrom((IEnumerable<string?>)values);

    private static MeasureYearByYearSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            ParseNullableDecimal(current),
            ParseNullableDecimal(previous),
            ParseNullableDecimal(previous2));

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

    internal static decimal? Average(params decimal?[] values) => Average((IEnumerable<decimal?>)values);

    internal static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
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

public record MeasureTopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false);

public record SchoolMeasureThreeYearAverage(
    decimal? SchoolValue,
    decimal? SimilarSchoolsValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record SchoolComparisonMeasureThreeYearAverage(
    decimal? CurrentSchoolValue,
    decimal? SimilarSchoolValue,
    decimal? EnglandValue);

public record MeasureYearByYearSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record SchoolMeasureYearByYear(
    MeasureYearByYearSeries School,
    MeasureYearByYearSeries SimilarSchools,
    MeasureYearByYearSeries LocalAuthority,
    MeasureYearByYearSeries England);

public record SchoolComparisonMeasureYearByYear(
    MeasureYearByYearSeries CurrentSchool,
    MeasureYearByYearSeries SimilarSchool,
    MeasureYearByYearSeries England);

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