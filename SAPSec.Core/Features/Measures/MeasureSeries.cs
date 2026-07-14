using SAPSec.Core.School.Similarity;

namespace SAPSec.Core.Features.Measures;

/// <summary>
/// Represents a series of data for a Measure for the current year and previous 2 years
/// </summary>
public record MeasureSeries(MeasureSeriesType SeriesType, decimal? Current, decimal? Previous, decimal? Previous2)
{
    internal static IReadOnlyCollection<MeasureSeries> ForSchool<T>(
        SchoolData<T> currentSchool,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolCurrent(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.SimilarSchoolsAverage,
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolCurrent(x.Data))),
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious(x.Data))),
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x.Data)))),
            new MeasureSeries(
                MeasureSeriesType.LASchoolsAverage,
                MeasureHelper.ParseNullableDecimal(fieldSelector.LocalAuthorityCurrent(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.LocalAuthorityPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandCurrent(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandPrevious(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandPrevious2(currentSchool.Data)))
        ];

    internal static IReadOnlyCollection<MeasureSeries> ForSchoolComparison<T>(
        SchoolData<T> currentSchool,
        SchoolData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolCurrent(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.SimilarSchool,
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolCurrent(similarSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious(similarSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolPrevious2(similarSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandCurrent(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandPrevious(currentSchool.Data)),
                MeasureHelper.ParseNullableDecimal(fieldSelector.EnglandPrevious2(currentSchool.Data))),
        ];
}

public enum MeasureSeriesType
{
    CurrentSchool,
    SimilarSchool,
    SimilarSchoolsAverage,
    LASchoolsAverage,
    EnglandSchoolsAverage
}