using SAPSec.Core.School.Secondary;

namespace SAPSec.Core.Measures;

public record YearByYearSubMeasure(
    IEnumerable<YearByYearSeries> Series) : SubMeasure
{
    internal static YearByYearSubMeasure ForSecondarySchool<T>(
        SecondarySchoolData<T> currentSchool,
        IEnumerable<SecondarySchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector) => new([
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            new YearByYearSeries(
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolCurrent(x.Data))),
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious(x.Data))),
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x.Data)))),
            MeasureHelper.SeriesFrom(
                fieldSelector.LocalAuthorityCurrent(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious(currentSchool.Data),
                fieldSelector.LocalAuthorityPrevious2(currentSchool.Data)),
            MeasureHelper.SeriesFrom(
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data))
        ]);

    internal static YearByYearSubMeasure ForSecondarySchoolComparison<T>(
        SecondarySchoolData<T> currentSchool,
        SecondarySchoolData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector) => new([
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(similarSchool.Data),
                fieldSelector.SchoolPrevious(similarSchool.Data),
                fieldSelector.SchoolPrevious2(similarSchool.Data)),
            MeasureHelper.SeriesFrom(
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data))
        ]);
}

public record YearByYearSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);