using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

internal static class AttendanceMeasures
{
    public static class TotalAbsence
    {
        public static Measure ForSchool(SchoolData<AbsenceData> currentSchool, IDictionary<string, string> filters)
        {
            var temp = Measure.ForSchool(
                Constants.Measures.Primary.TotalAbsence,
                MeasureDataType.AbsencePercentage,
                [],
                currentSchool,
                null,
                new(
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous2_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Eng_Current_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Eng_Previous_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Eng_Previous2_Pct));

            return temp;
        }
    }
}