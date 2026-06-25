using SAPSec.Core.Features.Primary;
using SAPSec.Core.Measures;

namespace SAPSec.Core.Features.Measures;

public record MeasureSeries(string Label, YearByYearSeries YearByYear, decimal? ThreeYearAverage)
{
    internal static IEnumerable<MeasureSeries> ForSecondarySchool<T>(
        SchoolData<T> currentSchool,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                currentSchool.SchoolInfo.Name,
                MeasureHelper.SeriesFrom(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                "Similar schools average",
                new YearByYearSeries(
                    MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolCurrent(x.Data))),
                    MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious(x.Data))),
                    MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x.Data)))),
                MeasureHelper.Average(similarSchools.Select(x => MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(x.Data),
                    fieldSelector.SchoolPrevious(x.Data),
                    fieldSelector.SchoolPrevious2(x.Data))))),
            new MeasureSeries(
                "Local authority schools average",
                MeasureHelper.SeriesFrom(
                    fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious2(currentSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                    fieldSelector.LocalAuthorityPrevious2(currentSchool.Data))),
            new MeasureSeries(
                "Schools in England average",
                MeasureHelper.SeriesFrom(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)))
        ];

    internal static IEnumerable<MeasureSeries> ForSecondarySchoolComparison<T>(
        SchoolData<T> currentSchool,
        SchoolData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector) => [
            new MeasureSeries(
                currentSchool.SchoolInfo.Name,
                MeasureHelper.SeriesFrom(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(currentSchool.Data),
                    fieldSelector.SchoolPrevious(currentSchool.Data),
                    fieldSelector.SchoolPrevious2(currentSchool.Data))),
            new MeasureSeries(
                similarSchool.SchoolInfo.Name,
                MeasureHelper.SeriesFrom(
                    fieldSelector.SchoolCurrent(similarSchool.Data),
                    fieldSelector.SchoolPrevious(similarSchool.Data),
                    fieldSelector.SchoolPrevious2(similarSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(similarSchool.Data),
                    fieldSelector.SchoolPrevious(similarSchool.Data),
                    fieldSelector.SchoolPrevious2(similarSchool.Data))),
            new MeasureSeries(
                "Schools in England average",
                MeasureHelper.SeriesFrom(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)),
                MeasureHelper.AverageFrom(
                    fieldSelector.EnglandCurrent(currentSchool.Data),
                    fieldSelector.EnglandPrevious(currentSchool.Data),
                    fieldSelector.EnglandPrevious2(currentSchool.Data)))
        ];
}
