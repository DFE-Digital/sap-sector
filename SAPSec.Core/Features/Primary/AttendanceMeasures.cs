using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

internal static class AttendanceMeasures
{
    public static class Absence
    {
        public static Measure ForSchool(SchoolData<AbsenceData> currentSchool, IDictionary<string, string> filters)
        {
            return Measure.ForSchool(
                Constants.Measures.Primary.OverallAbsence,
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
        }
    }

    //public static class PersistentAbsence
    //{
    //    public static Measure ForSchool(SchoolData<AbsenceData> currentSchool, IDictionary<string, string> filters)
    //    {
    //        return Measure.ForSchool(
    //            Constants.Measures.Primary.PersistentAbsence,
    //            MeasureDataType.AbsencePercentage,
    //            [],
    //            currentSchool,
    //            null,
    //            new(
    //                x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct,
    //                x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct,
    //                x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct,
    //                x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Current_Pct,
    //                x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous_Pct,
    //                x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous2_Pct,
    //                x => x?.EnglandAbsence?.Abs_Persistent_Eng_Current_Pct,
    //                x => x?.EnglandAbsence?.Abs_Persistent_Eng_Previous_Pct,
    //                x => x?.EnglandAbsence?.Abs_Persistent_Eng_Previous2_Pct));
    //    }
    //}
}