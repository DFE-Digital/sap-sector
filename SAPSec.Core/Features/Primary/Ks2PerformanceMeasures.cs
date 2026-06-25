using SAPSec.Core.Features.Measures;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Primary;

internal static class Ks2PerformanceMeasures
{
    public static class MeetingExpectedStandardRwm
    {
        public static Measure ForSecondarySchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSecondarySchool(
                "expected-rwm",
                "Meeting expected standard in reading, writing and maths",
                MeasureDataType.Score,
                [],
                currentSchool,
                similarSchools,
                new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous2_Num));
        }

        public static Measure ForSecondarySchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSecondarySchoolComparison(
                "expected-rwm",
                "Meeting expected standard in reading, writing and maths",
                MeasureDataType.Score,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous2_Num));
        }
    }
}
