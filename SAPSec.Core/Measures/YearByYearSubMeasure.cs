namespace SAPSec.Core.Measures;

public record YearByYearSubMeasure(
    IEnumerable<YearByYearSeries> Series) : SubMeasure
{
    internal static YearByYearSubMeasure ForSchool(
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector) => new([
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(schoolData),
                fieldSelector.SchoolPrevious(schoolData),
                fieldSelector.SchoolPrevious2(schoolData)),
            new YearByYearSeries(
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolCurrent(x))),
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious(x))),
                MeasureHelper.AverageFrom(similarSchools.Select(x => fieldSelector.SchoolPrevious2(x)))),
            MeasureHelper.SeriesFrom(
                fieldSelector.LocalAuthorityCurrent(schoolData),
                fieldSelector.LocalAuthorityPrevious(schoolData),
                fieldSelector.LocalAuthorityPrevious2(schoolData)),
            MeasureHelper.SeriesFrom(
                fieldSelector.EnglandCurrent(schoolData),
                fieldSelector.EnglandPrevious(schoolData),
                fieldSelector.EnglandPrevious2(schoolData))
        ]);

    internal static YearByYearSubMeasure ForSchoolComparison(
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        MeasureFieldSelector fieldSelector) => new([
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(currentSchoolData),
                fieldSelector.SchoolPrevious(currentSchoolData),
                fieldSelector.SchoolPrevious2(currentSchoolData)),
            MeasureHelper.SeriesFrom(
                fieldSelector.SchoolCurrent(similarSchoolData),
                fieldSelector.SchoolPrevious(similarSchoolData),
                fieldSelector.SchoolPrevious2(similarSchoolData)),
            MeasureHelper.SeriesFrom(
                fieldSelector.EnglandCurrent(currentSchoolData),
                fieldSelector.EnglandPrevious(currentSchoolData),
                fieldSelector.EnglandPrevious2(currentSchoolData))
        ]);
}

public record YearByYearSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);