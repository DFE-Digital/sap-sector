namespace SAPSec.Core.Features.Measures;

public record ThreeYearAverageSubMeasure(
    IEnumerable<decimal?> Averages) : SubMeasure
{
    internal static ThreeYearAverageSubMeasure ForSchool(
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector) => new([
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(schoolData),
                fieldSelector.SchoolPrevious(schoolData),
                fieldSelector.SchoolPrevious2(schoolData)),
            MeasureHelper.Average(similarSchools.Select(x => MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(x),
                fieldSelector.SchoolPrevious(x),
                fieldSelector.SchoolPrevious2(x)))),
            MeasureHelper.AverageFrom(
                fieldSelector.LocalAuthorityCurrent(schoolData),
                fieldSelector.LocalAuthorityPrevious(schoolData),
                fieldSelector.LocalAuthorityPrevious2(schoolData)),
            MeasureHelper.AverageFrom(
                fieldSelector.EnglandCurrent(schoolData),
                fieldSelector.EnglandPrevious(schoolData),
                fieldSelector.EnglandPrevious2(schoolData))
        ]);

    internal static ThreeYearAverageSubMeasure ForSchoolComparison(
        SchoolData currentSchool,
        SchoolData similarSchool,
        MeasureFieldSelector fieldSelector) => new([
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool),
                fieldSelector.SchoolPrevious(currentSchool),
                fieldSelector.SchoolPrevious2(currentSchool)),
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(similarSchool),
                fieldSelector.SchoolPrevious(similarSchool),
                fieldSelector.SchoolPrevious2(similarSchool)),
            MeasureHelper.AverageFrom(
                fieldSelector.EnglandCurrent(currentSchool),
                fieldSelector.EnglandPrevious(currentSchool),
                fieldSelector.EnglandPrevious2(currentSchool))
            ]);

}
