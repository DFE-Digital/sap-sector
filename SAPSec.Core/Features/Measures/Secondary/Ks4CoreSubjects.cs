using SAPSec.Core.Collections;
using SAPSec.Core.Filtering;
using SAPSec.Core.Text;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Features.Measures.Measures.Secondary;

namespace SAPSec.Core.Features.Measures.Secondary;

internal static class Ks4CoreSubjects
{
    public static class EnglishLanguage
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter)
        {
            var grade = filters.ContainsKey(Ks4EnglishLanguage.Filters.Grade.Key)
                ? filters[Ks4EnglishLanguage.Filters.Grade.Key]
                : Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove;

            var characteristic = includePupilCharacteristicFilter && filters.ContainsKey(Ks4EnglishLanguage.Filters.PupilCharacteristic.Key)
                ? filters[Ks4EnglishLanguage.Filters.PupilCharacteristic.Key]
                : Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.AllPupils;

            List<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishLanguage.Filters.Grade.Key,
                    Ks4EnglishLanguage.Filters.Grade.Name,
                    Ks4EnglishLanguage.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            if (includePupilCharacteristicFilter)
            {
                availableFilters.Add(new MeasureAvailableFilter(
                    Ks4EnglishLanguage.Filters.PupilCharacteristic.Key,
                    Ks4EnglishLanguage.Filters.PupilCharacteristic.Name,
                    Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList()));
            }

            var gradeCode = ResolveGradeCode(grade);
            var establishmentCharacteristicCode = ResolveEstablishmentCharacteristicCode(characteristic);
            var localAuthorityCharacteristicCode = ResolveLocalAuthorityCharacteristicCode(characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Current"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous2"));

            return (availableFilters, fieldSelector);
        }

        private static string ResolveGradeCode(string grade) =>
            grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove) => "59",
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove) => "79",
                _ => "49"
            };

        private static string ResolveEstablishmentCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.NonMobile) => "NMo",
                _ => "Sum"
            };

        private static string? ResolveLocalAuthorityCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLanguage.Filters.PupilCharacteristic.Values.NonMobile) => null,
                _ => "Tot"
            };

        private static string? Read(object? source, string gradeCode, string characteristicCode, string scope, string year)
        {
            if (source is null)
            {
                return null;
            }

            var propertyName = $"EngLang{gradeCode}_{characteristicCode}_{scope}_{year}_Pct";
            return source.GetType().GetProperty(propertyName)?.GetValue(source) as string;
        }
    }

    public static class EnglishLiterature
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter)
        {
            var grade = filters.ContainsKey(Ks4EnglishLiterature.Filters.Grade.Key)
                ? filters[Ks4EnglishLiterature.Filters.Grade.Key]
                : Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove;

            var characteristic = includePupilCharacteristicFilter && filters.ContainsKey(Ks4EnglishLiterature.Filters.PupilCharacteristic.Key)
                ? filters[Ks4EnglishLiterature.Filters.PupilCharacteristic.Key]
                : Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.AllPupils;

            List<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishLiterature.Filters.Grade.Key,
                    Ks4EnglishLiterature.Filters.Grade.Name,
                    Ks4EnglishLiterature.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            if (includePupilCharacteristicFilter)
            {
                availableFilters.Add(new MeasureAvailableFilter(
                    Ks4EnglishLiterature.Filters.PupilCharacteristic.Key,
                    Ks4EnglishLiterature.Filters.PupilCharacteristic.Name,
                    Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList()));
            }

            var gradeCode = ResolveGradeCode(grade);
            var establishmentCharacteristicCode = ResolveEstablishmentCharacteristicCode(characteristic);
            var localAuthorityCharacteristicCode = ResolveLocalAuthorityCharacteristicCode(characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Current"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous2"));

            return (availableFilters, fieldSelector);
        }

        private static string ResolveGradeCode(string grade) =>
            grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove) => "59",
                _ when grade.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove) => "79",
                _ => "49"
            };

        private static string ResolveEstablishmentCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.NonMobile) => "NMo",
                _ => "Sum"
            };

        private static string? ResolveLocalAuthorityCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishLiterature.Filters.PupilCharacteristic.Values.NonMobile) => null,
                _ => "Tot"
            };

        private static string? Read(object? source, string gradeCode, string characteristicCode, string scope, string year)
        {
            if (source is null)
            {
                return null;
            }

            var propertyName = $"EngLit{gradeCode}_{characteristicCode}_{scope}_{year}_Pct";
            return source.GetType().GetProperty(propertyName)?.GetValue(source) as string;
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
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter)
        {
            var grade = filters.ContainsKey(Ks4Maths.Filters.Grade.Key)
                ? filters[Ks4Maths.Filters.Grade.Key]
                : Ks4Maths.Filters.Grade.Values.Grade4AndAbove;

            var characteristic = includePupilCharacteristicFilter && filters.ContainsKey(Ks4Maths.Filters.PupilCharacteristic.Key)
                ? filters[Ks4Maths.Filters.PupilCharacteristic.Key]
                : Ks4Maths.Filters.PupilCharacteristic.Values.AllPupils;

            List<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Maths.Filters.Grade.Key,
                    Ks4Maths.Filters.Grade.Name,
                    Ks4Maths.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            if (includePupilCharacteristicFilter)
            {
                availableFilters.Add(new MeasureAvailableFilter(
                    Ks4Maths.Filters.PupilCharacteristic.Key,
                    Ks4Maths.Filters.PupilCharacteristic.Name,
                    Ks4Maths.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList()));
            }

            var gradeCode = ResolveGradeCode(grade);
            var establishmentCharacteristicCode = ResolveEstablishmentCharacteristicCode(characteristic);
            var localAuthorityCharacteristicCode = ResolveLocalAuthorityCharacteristicCode(characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Current"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous2"));

            return (availableFilters, fieldSelector);
        }

        private static string ResolveGradeCode(string grade) =>
            grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4Maths.Filters.Grade.Values.Grade5AndAbove) => "59",
                _ when grade.EqualsCaseInsensitive(Ks4Maths.Filters.Grade.Values.Grade7AndAbove) => "79",
                _ => "49"
            };

        private static string ResolveEstablishmentCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.NonMobile) => "NMo",
                _ => "Sum"
            };

        private static string? ResolveLocalAuthorityCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4Maths.Filters.PupilCharacteristic.Values.NonMobile) => null,
                _ => "Tot"
            };

        private static string? Read(object? source, string gradeCode, string characteristicCode, string scope, string year)
        {
            if (source is null)
            {
                return null;
            }

            var propertyName = $"Maths{gradeCode}_{characteristicCode}_{scope}_{year}_Pct";
            return source.GetType().GetProperty(propertyName)?.GetValue(source) as string;
        }
    }

    public static class CombinedScience
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter = true)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters, includePupilCharacteristicFilter);

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

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters, bool includePupilCharacteristicFilter)
        {
            var grade = filters.ContainsKey(Ks4CombinedScience.Filters.Grade.Key)
                ? filters[Ks4CombinedScience.Filters.Grade.Key]
                : Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove;

            var characteristic = includePupilCharacteristicFilter && filters.ContainsKey(Ks4CombinedScience.Filters.PupilCharacteristic.Key)
                ? filters[Ks4CombinedScience.Filters.PupilCharacteristic.Key]
                : Ks4CombinedScience.Filters.PupilCharacteristic.Values.AllPupils;

            List<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4CombinedScience.Filters.Grade.Key,
                    Ks4CombinedScience.Filters.Grade.Name,
                    Ks4CombinedScience.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            if (includePupilCharacteristicFilter)
            {
                availableFilters.Add(new MeasureAvailableFilter(
                    Ks4CombinedScience.Filters.PupilCharacteristic.Key,
                    Ks4CombinedScience.Filters.PupilCharacteristic.Name,
                    Ks4CombinedScience.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList()));
            }

            var gradeCode = ResolveGradeCode(grade);
            var establishmentCharacteristicCode = ResolveEstablishmentCharacteristicCode(characteristic);
            var localAuthorityCharacteristicCode = ResolveLocalAuthorityCharacteristicCode(characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Current"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous"),
                x => Read(x?.EstablishmentPerformance, gradeCode, establishmentCharacteristicCode, "Est", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.LocalAuthorityPerformance, gradeCode, localAuthorityCharacteristicCode, "LA", "Previous2"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Current"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous"),
                x => localAuthorityCharacteristicCode is null ? null : Read(x?.EnglandPerformance, gradeCode, localAuthorityCharacteristicCode, "Eng", "Previous2"));

            return (availableFilters, fieldSelector);
        }

        private static string ResolveGradeCode(string grade) =>
            grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove) => "59",
                _ when grade.EqualsCaseInsensitive(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove) => "79",
                _ => "49"
            };

        private static string ResolveEstablishmentCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.NonMobile) => "NMo",
                _ => "Sum"
            };

        private static string? ResolveLocalAuthorityCharacteristicCode(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Boys) => "Boy",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Girls) => "Grl",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Disadvantaged) => "Dis",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.NonDisadvantaged) => "NDi",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.Eal) => "EAL",
                _ when characteristic.EqualsCaseInsensitive(Ks4CombinedScience.Filters.PupilCharacteristic.Values.NonMobile) => null,
                _ => "Tot"
            };

        private static string? Read(object? source, string gradeCode, string characteristicCode, string scope, string year)
        {
            if (source is null)
            {
                return null;
            }

            var propertyName = $"CombSci{gradeCode}_{characteristicCode}_{scope}_{year}_Pct";
            return source.GetType().GetProperty(propertyName)?.GetValue(source) as string;
        }
    }
}
