using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.Measures;

/// <summary>
/// Represents a series of data for the current, previous and previous 2 years, and average over the 3 years
/// </summary>
public record MeasureSeries(MeasureSeriesType SeriesType, YearByYearSeries YearByYear, decimal? ThreeYearAverage)
{
    internal static IReadOnlyCollection<MeasureSeries> ForSchool<T>(
        SchoolData<T> currentSchool,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                YearByYearSeries.FromStringValues(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.SimilarSchoolsAverage,
                new YearByYearSeries(
                    MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolCurrent(x.Data))),
                    MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious(x.Data))),
                    MeasureHelper.Average(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x.Data)))),
                MeasureHelper.Average(similarSchools.Select(x => MeasureHelper.Average(
                    fieldSelector.SchoolCurrent(x.Data),
                    fieldSelector.SchoolPrevious(x.Data),
                    fieldSelector.SchoolPrevious2(x.Data))))),
            new MeasureSeries(
                MeasureSeriesType.LASchoolsAverage,
                YearByYearSeries.FromStringValues(
                    fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious2(currentSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                YearByYearSeries.FromStringValues(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)))
        ];

    internal static IReadOnlyCollection<MeasureSeries> ForSchoolComparison<T>(
        SchoolData<T> currentSchool,
        SchoolData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                MeasureSeriesType.CurrentSchool,
                YearByYearSeries.FromStringValues(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.SimilarSchool,
                YearByYearSeries.FromStringValues(
                    fieldSelector.SchoolCurrent(similarSchool.Data),
                    fieldSelector.SchoolPrevious(similarSchool.Data),
                    fieldSelector.SchoolPrevious2(similarSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.SchoolCurrent(similarSchool.Data),
                    fieldSelector.SchoolPrevious(similarSchool.Data),
                    fieldSelector.SchoolPrevious2(similarSchool.Data))),
            new MeasureSeries(
                MeasureSeriesType.EnglandSchoolsAverage,
                YearByYearSeries.FromStringValues(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)),
                MeasureHelper.Average(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)))
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