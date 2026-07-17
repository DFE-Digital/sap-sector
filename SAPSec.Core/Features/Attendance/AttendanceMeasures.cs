using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

internal static class AttendanceMeasures
{
    public static class TotalAbsence
    {
        public static Measure ForSchool(SchoolData<AbsenceData> currentSchool,
            IEnumerable<SchoolData<AbsenceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSchool(
                Constants.Measures.Primary.TotalAbsence,
                MeasureDataType.AbsencePercentage,
                [],
                currentSchool,
                similarSchools,
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

        public static Measure ForSchoolComparison(SchoolData<AbsenceData> currentSchool,
            SchoolData<AbsenceData> similarSchool,
            IEnumerable<SchoolData<AbsenceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSchoolComparison(
                Constants.Measures.Primary.TotalAbsence,
                MeasureDataType.AbsencePercentage,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
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

    public static class PersistentAbsence
    {
        public static Measure ForSchool(SchoolData<AbsenceData> currentSchool,
            IEnumerable<SchoolData<AbsenceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSchool(
                Constants.Measures.Primary.PersistentAbsence,
                MeasureDataType.AbsencePercentage,
                [],
                currentSchool,
                similarSchools,
                new(
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Current_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous2_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Eng_Current_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Eng_Previous_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData<AbsenceData> currentSchool,
            SchoolData<AbsenceData> similarSchool,
            IEnumerable<SchoolData<AbsenceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSchoolComparison(
                Constants.Measures.Primary.PersistentAbsence,
                MeasureDataType.AbsencePercentage,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
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
}





// //ForSchool
//var overallSchoolSeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct),
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct),
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct));
// var persistentSchoolSeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct),
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct),
//     ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct));

// var overallLocalAuthoritySeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Current_Pct),
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous_Pct),
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous2_Pct));
// var persistentLocalAuthoritySeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Current_Pct),
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous_Pct),
//     ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous2_Pct));

// var overallEnglandSeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Current_Pct),
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous_Pct),
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous2_Pct));
// var persistentEnglandSeries = new AttendanceMeasureSeries(
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Current_Pct),
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous_Pct),
//     ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous2_Pct));


// //ForSchoolComparison
// var overallSimilarSchoolsSeries = new AttendanceMeasureSeries(
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct))),
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct))),
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct))));
// var persistentSimilarSchoolsSeries = new AttendanceMeasureSeries(
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct))),
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct))),
//     AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct))));

//     return new (
//         new AttendanceMeasureAverage(
//             overallSchoolSeries.Current,
//             AverageAvailable(similarSchoolData.Select(x =>
//                 ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct))),
//             overallLocalAuthoritySeries.Current,
//             overallEnglandSeries.Current),
//         BuildTopPerformers(
//             establishment,
//             overallSchoolSeries.Current,
//             similarSchoolMeasures,
//             x => ParseNullableDecimal(x.AbsenceData?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct)),
//         new AttendanceMeasureYearByYear(
//             overallSchoolSeries,
//             overallSimilarSchoolsSeries,
//             overallLocalAuthoritySeries,
//             overallEnglandSeries),
//         new AttendanceMeasureAverage(
//             persistentSchoolSeries.Current,
//             AverageAvailable(similarSchoolData.Select(x =>
//                 ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct))),
//             persistentLocalAuthoritySeries.Current,
//             persistentEnglandSeries.Current),
//         BuildTopPerformers(
//             establishment,
//             persistentSchoolSeries.Current,
//             similarSchoolMeasures,
//             x => ParseNullableDecimal(x.AbsenceData?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct)),
//         new AttendanceMeasureYearByYear(
//             persistentSchoolSeries,
//             persistentSimilarSchoolsSeries,
//             persistentLocalAuthoritySeries,
//             persistentEnglandSeries));






