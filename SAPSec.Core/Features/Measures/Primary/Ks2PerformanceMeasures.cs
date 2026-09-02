using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Features.Measures.Measures.Primary;

namespace SAPSec.Core.Features.Measures.Primary;

internal static class Ks2PerformanceMeasures
{
    public static class MeetingExpectedStandardRwm
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2ExpectedRwm.Key,
                Ks2ExpectedRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2ExpectedRwm.Key,
                Ks2ExpectedRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
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

    public static class AchievedHigherStandardRwm
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2HigherRwm.Key,
                Ks2HigherRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2HigherRwm.Key,
                Ks2HigherRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var subject = filters.ContainsKey(Ks2HigherRwm.Filters.Subject.Key)
                ? filters[Ks2HigherRwm.Filters.Subject.Key]
                : Ks2HigherRwm.Filters.Subject.Values.ReadingWritingMaths;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2HigherRwm.Filters.Subject.Key,
                    Ks2HigherRwm.Filters.Subject.Name,
                    Ks2HigherRwm.Filters.Subject.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(subject)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Reading) => new(
                    x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Previous2_Num),
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Writing) => new(
                    x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Previous2_Num),
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Maths) => new(
                    x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Previous2_Num),
                _ => new(
                    x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class AverageScaledScoreReading
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2ReadingScore.Key,
                Ks2ReadingScore.Name,
                2024,
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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2ReadingScore.Key,
                Ks2ReadingScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchool,
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
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2MathsScore.Key,
                Ks2MathsScore.Name,
                2024,
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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2MathsScore.Key,
                Ks2MathsScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                [],
                currentSchool,
                similarSchool,
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

    public static class MeetingExpectedStandardGps
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2ExpectedGps.Key,
                Ks2ExpectedGps.Name,
                2024,
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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2ExpectedGps.Key,
                Ks2ExpectedGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchool,
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

    public static class AchievedHigherStandardGps
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchool(
                Ks2HigherGps.Key,
                Ks2HigherGps.Name,
                2024,
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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            return Measure.ForSchoolComparison(
                Ks2HigherGps.Key,
                Ks2HigherGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                [],
                currentSchool,
                similarSchool,
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
