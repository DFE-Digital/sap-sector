using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Constants.Measures.Primary;

namespace SAPSec.Core.Features.Primary;

internal static class Ks2PerformanceMeasures
{
    public static class MeetingExpectedStandardRwm
    {
        public static Measure ForSchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2ExpectedRwm.Key,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2ExpectedRwm.Key,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                similarSchools,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var subject = filters.ContainsKey(Ks2ExpectedRwm.Filters.Subject.Key)
                ? filters[Ks2ExpectedRwm.Filters.Subject.Key]
                : Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2ExpectedRwm.Filters.Subject.Key,
                    Ks2ExpectedRwm.Filters.Subject.Name,
                    Ks2ExpectedRwm.Filters.Subject.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(subject)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Reading) => new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num),
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Writing) => new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num),
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Maths) => new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num),
                _ => new(
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class MeetingExpectedStandardGps
    {
        public static Measure ForSchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2GpsExpected,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous2_Num));
        }

        public static Measure ForSchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2GpsExpected,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous2_Num));
        }
    }

    public static class AverageScaledScoreReading
    {
        public static Measure ForSchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2ReadingScore,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num));
        }

        public static Measure ForSchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2ReadingScore,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num));
        }
    }

    public static class AverageScaledScoreMaths
    {
        public static Measure ForSchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2MathsScore,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous2_Num));
        }

        public static Measure ForSchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2MathsScore,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous2_Num));
        }
    }

    public static class AchievedHigherStandardGps
    {
        public static Measure ForSchool(SchoolData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2GpsHigher,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous2_Num));
        }

        public static Measure ForSchoolComparison(SchoolData<Ks2PerformanceData> currentSchool, SchoolData<Ks2PerformanceData> similarSchool, IEnumerable<SchoolData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2GpsHigher,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new MeasureFieldSelector<Ks2PerformanceData>(
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous2_Num));
        }
    }
}
