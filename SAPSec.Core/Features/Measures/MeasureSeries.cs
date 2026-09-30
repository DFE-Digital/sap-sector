using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

/// <summary>
/// Represents a series of data for a Measure for the current year and previous 2 years
/// </summary>
public record MeasureSeries(MeasureSeriesType SeriesType, decimal? Current, decimal? Previous, decimal? Previous2)
{
    internal static IReadOnlyCollection<MeasureSeries> ForSchool<T>(
        SchoolMeasureData<T> currentSchool,
        IEnumerable<SchoolMeasureData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.SimilarSchoolsAverage,
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolCurrent(x.Data))),
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious(x.Data))),
                MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x.Data)))),
            new MeasureSeries(
                MeasureSeriesType.LASchoolsAverage,
                fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious2(currentSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data))
        ];

    //No similar schools average for attendance measures on school overview page
    internal static IReadOnlyCollection<MeasureSeries> ForSchoolAttendance<T>(
        SchoolMeasureData<T> currentSchool,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData => [
        new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.LASchoolsAverage,
                fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious2(currentSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data))
    ];

    internal static IReadOnlyCollection<MeasureSeries> ForSchoolComparison<T>(
        SchoolMeasureData<T> currentSchool,
        SchoolMeasureData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector)
        where T : class, IMeasureData => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.ComparatorSchool,
                fieldSelector.SchoolCurrent(similarSchool.Data),
                fieldSelector.SchoolPrevious(similarSchool.Data),
                fieldSelector.SchoolPrevious2(similarSchool.Data)),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data)),
        ];
}

public enum MeasureSeriesType
{
    CurrentSchool,
    ComparatorSchool,
    SimilarSchoolsAverage,
    LASchoolsAverage,
    EnglandSchoolsAverage
}