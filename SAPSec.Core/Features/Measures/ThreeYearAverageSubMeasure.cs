using SAPSec.Core.Features.Primary;
using SAPSec.Core.Measures;

namespace SAPSec.Core.Features.Measures;

public record ThreeYearAverageSubMeasure(
    IEnumerable<decimal?> Averages) : SubMeasure
{
    internal static ThreeYearAverageSubMeasure ForSecondarySchool<T>(
        SchoolData<T> schoolData,
        IEnumerable<SchoolData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector) => new([
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(schoolData.Data),
                fieldSelector.SchoolPrevious(schoolData.Data),
                fieldSelector.SchoolPrevious2(schoolData.Data)),
            MeasureHelper.Average(similarSchools.Select(x => MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(x.Data),
                fieldSelector.SchoolPrevious(x.Data),
                fieldSelector.SchoolPrevious2(x.Data)))),
            MeasureHelper.AverageFrom(
                fieldSelector.LocalAuthorityCurrent(schoolData.Data),
                fieldSelector.LocalAuthorityPrevious(schoolData.Data),
                fieldSelector.LocalAuthorityPrevious2(schoolData.Data)),
            MeasureHelper.AverageFrom(
                fieldSelector.EnglandCurrent(schoolData.Data),
                fieldSelector.EnglandPrevious(schoolData.Data),
                fieldSelector.EnglandPrevious2(schoolData.Data))
        ]);

    internal static ThreeYearAverageSubMeasure ForSecondarySchoolComparison<T>(
        SchoolData<T> currentSchool,
        SchoolData<T> similarSchool,
        MeasureFieldSelector<T> fieldSelector) => new([
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(similarSchool.Data),
                fieldSelector.SchoolPrevious(similarSchool.Data),
                fieldSelector.SchoolPrevious2(similarSchool.Data)),
            MeasureHelper.AverageFrom(
                fieldSelector.EnglandCurrent(currentSchool.Data),
                fieldSelector.EnglandPrevious(currentSchool.Data),
                fieldSelector.EnglandPrevious2(currentSchool.Data))
            ]);

}
