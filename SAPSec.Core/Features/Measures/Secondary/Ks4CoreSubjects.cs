using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Constants.Measures.Secondary;

namespace SAPSec.Core.Features.Measures.Secondary;

internal static class Ks4CoreSubjects
{
    public static class EnglishLanguage
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4EnglishLanguage.Key,
                Ks4EnglishLanguage.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4EnglishLanguage.Key,
                Ks4EnglishLanguage.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4EnglishLanguage.Filters.Grade.Key)
                ? filters[Ks4EnglishLanguage.Filters.Grade.Key]
                : Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishLanguage.Filters.Grade.Key,
                    Ks4EnglishLanguage.Filters.Grade.Name,
                    Ks4EnglishLanguage.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class EnglishLiterature
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4EnglishLiterature.Key,
                Ks4EnglishLiterature.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4EnglishLiterature.Key,
                Ks4EnglishLiterature.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4EnglishLiterature.Filters.Grade.Key)
                ? filters[Ks4EnglishLiterature.Filters.Grade.Key]
                : Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishLiterature.Filters.Grade.Key,
                    Ks4EnglishLiterature.Filters.Grade.Name,
                    Ks4EnglishLiterature.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class Biology
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Biology.Key,
                Ks4Biology.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Biology.Key,
                Ks4Biology.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4Biology.Filters.Grade.Key)
                ? filters[Ks4Biology.Filters.Grade.Key]
                : Ks4Biology.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Biology.Filters.Grade.Key,
                    Ks4Biology.Filters.Grade.Name,
                    Ks4Biology.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4Biology.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4Biology.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class Chemistry
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Chemistry.Key,
                Ks4Chemistry.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Chemistry.Key,
                Ks4Chemistry.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4Chemistry.Filters.Grade.Key)
                ? filters[Ks4Chemistry.Filters.Grade.Key]
                : Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Chemistry.Filters.Grade.Key,
                    Ks4Chemistry.Filters.Grade.Name,
                    Ks4Chemistry.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class Physics
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Physics.Key,
                Ks4Physics.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Physics.Key,
                Ks4Physics.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4Physics.Filters.Grade.Key)
                ? filters[Ks4Physics.Filters.Grade.Key]
                : Ks4Physics.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Physics.Filters.Grade.Key,
                    Ks4Physics.Filters.Grade.Name,
                    Ks4Physics.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4Physics.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4Physics.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class Maths
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Maths.Key,
                Ks4Maths.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Maths.Key,
                Ks4Maths.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4Maths.Filters.Grade.Key)
                ? filters[Ks4Maths.Filters.Grade.Key]
                : Ks4Maths.Filters.Grade.Values.Grade4AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Maths.Filters.Grade.Key,
                    Ks4Maths.Filters.Grade.Name,
                    Ks4Maths.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4Maths.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4Maths.Filters.Grade.Values.Grade7AndAbove) => new(
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class CombinedScience
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4CombinedScience.Key,
                Ks4CombinedScience.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4CombinedScience.Key,
                Ks4CombinedScience.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4CombinedScience.Filters.Grade.Key)
                ? filters[Ks4CombinedScience.Filters.Grade.Key]
                : Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4CombinedScience.Filters.Grade.Key,
                    Ks4CombinedScience.Filters.Grade.Name,
                    Ks4CombinedScience.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove) => new(
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),

                _ when grade.EqualsCaseInsensitive(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove) => new(
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }
}