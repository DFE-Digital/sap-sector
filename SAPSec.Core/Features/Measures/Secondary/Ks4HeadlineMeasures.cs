using SAPSec.Core.Collections;
using SAPSec.Core.Filtering;
using SAPSec.Core.Text;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Features.Measures.Measures.Secondary;

namespace SAPSec.Core.Features.Measures.Secondary;

internal static class Ks4HeadlineMeasures
{
    public static class Attainment8
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Attainment8.Key,
                Ks4Attainment8.Name,
                2024,
                MeasureDataType.Score,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4PerformanceData> currentSchool, SchoolMeasureData<Ks4PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Attainment8.Key,
                Ks4Attainment8.Name,
                2024,
                MeasureDataType.Score,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var characteristic = filters.ContainsKey(Ks4Attainment8.Filters.PupilCharacteristic.Key)
                ? filters[Ks4Attainment8.Filters.PupilCharacteristic.Key]
                : Ks4Attainment8.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Attainment8.Filters.PupilCharacteristic.Key,
                    Ks4Attainment8.Filters.PupilCharacteristic.Name,
                    Ks4Attainment8.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(characteristic);
            var (laCurrent, laPrevious, laPrevious2, englandCurrent, englandPrevious, englandPrevious2) =
                ResolveLocalAuthorityAndEnglandAccessors(characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector);
        }

        private static (
            Func<Ks4PerformanceData?, string?> Current,
            Func<Ks4PerformanceData?, string?> Previous,
            Func<Ks4PerformanceData?, string?> Previous2) ResolveEstablishmentAccessors(string characteristic) =>
            characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Boys) => (
                    x => x?.EstablishmentPerformance?.Attainment8_Boy_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Boy_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Boy_Est_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Girls) => (
                    x => x?.EstablishmentPerformance?.Attainment8_Grl_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Grl_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Grl_Est_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                    x => x?.EstablishmentPerformance?.Attainment8_Dis_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Dis_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Dis_Est_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                    x => x?.EstablishmentPerformance?.Attainment8_NDi_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_NDi_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_NDi_Est_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Eal) => (
                    x => x?.EstablishmentPerformance?.Attainment8_EAL_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_EAL_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_EAL_Est_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.NonMobile) => (
                    x => x?.EstablishmentPerformance?.Attainment8_NMo_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_NMo_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_NMo_Est_Previous2_Num),
                _ => (
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num)
            };

        private static (
            Func<Ks4PerformanceData?, string?> LACurrent,
            Func<Ks4PerformanceData?, string?> LAPrevious,
            Func<Ks4PerformanceData?, string?> LAPrevious2,
            Func<Ks4PerformanceData?, string?> EnglandCurrent,
            Func<Ks4PerformanceData?, string?> EnglandPrevious,
            Func<Ks4PerformanceData?, string?> EnglandPrevious2) ResolveLocalAuthorityAndEnglandAccessors(string characteristic)
        {
            if (characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.NonMobile))
            {
                return (x => null, x => null, x => null, x => null, x => null, x => null);
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Boys) => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_Boy_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Boy_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Boy_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Boy_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Boy_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Boy_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Girls) => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_Grl_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Grl_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Grl_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Grl_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Grl_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Grl_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_Dis_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Dis_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Dis_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Dis_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Dis_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Dis_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_NDi_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_NDi_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_NDi_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_NDi_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_NDi_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_NDi_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks4Attainment8.Filters.PupilCharacteristic.Values.Eal) => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_EAL_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_EAL_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_EAL_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_EAL_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_EAL_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_EAL_Eng_Previous2_Num),
                _ => (
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)
            };
        }
    }

    public static class EnglishMaths
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks4PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4EnglishMaths.Key,
                Ks4EnglishMaths.Name,
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
                Ks4EnglishMaths.Key,
                Ks4EnglishMaths.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var grade = filters.ContainsKey(Ks4EnglishMaths.Filters.Grade.Key)
                ? filters[Ks4EnglishMaths.Filters.Grade.Key]
                : Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove;

            var characteristic = filters.ContainsKey(Ks4EnglishMaths.Filters.PupilCharacteristic.Key)
                ? filters[Ks4EnglishMaths.Filters.PupilCharacteristic.Key]
                : Ks4EnglishMaths.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishMaths.Filters.Grade.Key,
                    Ks4EnglishMaths.Filters.Grade.Name,
                    Ks4EnglishMaths.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList()),
                new MeasureAvailableFilter(
                    Ks4EnglishMaths.Filters.PupilCharacteristic.Key,
                    Ks4EnglishMaths.Filters.PupilCharacteristic.Name,
                    Ks4EnglishMaths.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var isGrade5 = grade.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove);
            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(isGrade5, characteristic);
            var (laCurrent, laPrevious, laPrevious2, englandCurrent, englandPrevious, englandPrevious2) =
                ResolveLocalAuthorityAndEnglandAccessors(isGrade5, characteristic);

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector);
        }

        private static (
            Func<Ks4PerformanceData?, string?> Current,
            Func<Ks4PerformanceData?, string?> Previous,
            Func<Ks4PerformanceData?, string?> Previous2) ResolveEstablishmentAccessors(bool isGrade5, string characteristic)
        {
            if (isGrade5)
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_Boy_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Boy_Est_Previous_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_Grl_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Grl_Est_Previous_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_Dis_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Dis_Est_Previous_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_NDi_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_NDi_Est_Previous_Pct,
                        x => null),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_EAL_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_EAL_Est_Previous_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_EAL_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.EngMaths59_NMo_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_NMo_Est_Previous_Pct,
                        x => null),
                    _ => (
                        x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                        x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct)
                };
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Boys) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_Boy_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Boy_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Boy_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Girls) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_Grl_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Grl_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Grl_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_Dis_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Dis_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Dis_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_NDi_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_NDi_Est_Previous_Pct,
                    x => null),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Eal) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_EAL_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_EAL_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_EAL_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonMobile) => (
                    x => x?.EstablishmentPerformance?.EngMaths49_NMo_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_NMo_Est_Previous_Pct,
                    x => null),
                _ => (
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct)
            };
        }

        private static (
            Func<Ks4PerformanceData?, string?> LACurrent,
            Func<Ks4PerformanceData?, string?> LAPrevious,
            Func<Ks4PerformanceData?, string?> LAPrevious2,
            Func<Ks4PerformanceData?, string?> EnglandCurrent,
            Func<Ks4PerformanceData?, string?> EnglandPrevious,
            Func<Ks4PerformanceData?, string?> EnglandPrevious2) ResolveLocalAuthorityAndEnglandAccessors(bool isGrade5, string characteristic)
        {
            if (characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonMobile))
            {
                return (x => null, x => null, x => null, x => null, x => null, x => null);
            }

            if (isGrade5)
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Boy_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Boy_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Boy_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Boy_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Boy_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Boy_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Grl_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Grl_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Grl_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Grl_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Grl_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Grl_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Dis_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Dis_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Dis_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Dis_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Dis_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Dis_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_NDi_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_NDi_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_NDi_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_NDi_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_NDi_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_NDi_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_EAL_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_EAL_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_EAL_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_EAL_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_EAL_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_EAL_Eng_Previous2_Pct),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct,
                        x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct,
                        x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)
                };
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Boys) => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Boy_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Boy_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Boy_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Boy_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Boy_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Boy_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Girls) => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Grl_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Grl_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Grl_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Grl_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Grl_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Grl_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Dis_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Dis_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Dis_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Dis_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Dis_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Dis_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_NDi_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_NDi_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_NDi_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_NDi_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_NDi_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_NDi_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.PupilCharacteristic.Values.Eal) => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_EAL_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_EAL_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_EAL_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_EAL_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_EAL_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_EAL_Eng_Previous2_Pct),
                _ => (
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)
            };
        }
    }

    public static class Destinations
    {
        public static Measure ForSchool(SchoolMeasureData<Ks4DestinationsData> currentSchool, IEnumerable<SchoolMeasureData<Ks4DestinationsData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks4Destinations.Key,
                Ks4Destinations.Name,
                2022,
                MeasureDataType.DestinationsPercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks4DestinationsData> currentSchool, SchoolMeasureData<Ks4DestinationsData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks4Destinations.Key,
                Ks4Destinations.Name,
                2022,
                MeasureDataType.DestinationsPercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks4DestinationsData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var destination = filters.ContainsKey(Ks4Destinations.Filters.Destination.Key)
                ? filters[Ks4Destinations.Filters.Destination.Key]
                : Ks4Destinations.Filters.Destination.Values.AllDestinations;

            var characteristic = filters.ContainsKey(Ks4Destinations.Filters.PupilCharacteristic.Key)
                ? filters[Ks4Destinations.Filters.PupilCharacteristic.Key]
                : Ks4Destinations.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Destinations.Filters.Destination.Key,
                    Ks4Destinations.Filters.Destination.Name,
                    Ks4Destinations.Filters.Destination.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(destination)))
                    .ToList()),
                new MeasureAvailableFilter(
                    Ks4Destinations.Filters.PupilCharacteristic.Key,
                    Ks4Destinations.Filters.PupilCharacteristic.Name,
                    Ks4Destinations.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(destination, characteristic);
            var (laCurrent, laPrevious, laPrevious2) = ResolveLocalAuthorityAccessors(destination, characteristic);
            var (englandCurrent, englandPrevious, englandPrevious2) = ResolveEnglandAccessors(destination, characteristic);

            MeasureFieldSelector<Ks4DestinationsData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector);
        }

        private static (
            Func<Ks4DestinationsData?, string?> Current,
            Func<Ks4DestinationsData?, string?> Previous,
            Func<Ks4DestinationsData?, string?> Previous2) ResolveEstablishmentAccessors(string destination, string characteristic)
        {
            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Education))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EstablishmentDestinations?.Education_Boy_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_Boy_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EstablishmentDestinations?.Education_Grl_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_Grl_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EstablishmentDestinations?.Education_Dis_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_Dis_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EstablishmentDestinations?.Education_NDi_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_NDi_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_NDi_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EstablishmentDestinations?.Education_EAL_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_EAL_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_EAL_Est_Previous2_Pct),
                    _ => (x => x?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct, x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Apprenticeships))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EstablishmentDestinations?.Apprentice_Boy_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_Boy_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EstablishmentDestinations?.Apprentice_Grl_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_Grl_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EstablishmentDestinations?.Apprentice_Dis_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_Dis_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EstablishmentDestinations?.Apprentice_NDi_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_NDi_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_NDi_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EstablishmentDestinations?.Apprentice_EAL_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_EAL_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_EAL_Est_Previous2_Pct),
                    _ => (x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Current_Pct, x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Employment))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EstablishmentDestinations?.Employment_Boy_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_Boy_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EstablishmentDestinations?.Employment_Grl_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_Grl_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EstablishmentDestinations?.Employment_Dis_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_Dis_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EstablishmentDestinations?.Employment_NDi_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_NDi_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_NDi_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EstablishmentDestinations?.Employment_EAL_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_EAL_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_EAL_Est_Previous2_Pct),
                    _ => (x => x?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct, x => x?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct, x => x?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct)
                };
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EstablishmentDestinations?.AllDest_Boy_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_Boy_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_Boy_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EstablishmentDestinations?.AllDest_Grl_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_Grl_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_Grl_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EstablishmentDestinations?.AllDest_Dis_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_Dis_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_Dis_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EstablishmentDestinations?.AllDest_NDi_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_NDi_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_NDi_Est_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EstablishmentDestinations?.AllDest_EAL_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_EAL_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_EAL_Est_Previous2_Pct),
                _ => (x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct, x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct, x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct)
            };
        }

        private static (
            Func<Ks4DestinationsData?, string?> Current,
            Func<Ks4DestinationsData?, string?> Previous,
            Func<Ks4DestinationsData?, string?> Previous2) ResolveLocalAuthorityAccessors(string destination, string characteristic)
        {
            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Education))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.LocalAuthorityDestinations?.Education_Boy_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_Boy_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_Boy_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.LocalAuthorityDestinations?.Education_Grl_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_Grl_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_Grl_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.LocalAuthorityDestinations?.Education_Dis_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_Dis_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_Dis_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.LocalAuthorityDestinations?.Education_NDi_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_NDi_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_NDi_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.LocalAuthorityDestinations?.Education_EAL_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_EAL_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_EAL_LA_Previous2_Pct),
                    _ => (x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Apprenticeships))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.LocalAuthorityDestinations?.Apprentice_Boy_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Boy_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Boy_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.LocalAuthorityDestinations?.Apprentice_Grl_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Grl_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Grl_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.LocalAuthorityDestinations?.Apprentice_Dis_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Dis_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Dis_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.LocalAuthorityDestinations?.Apprentice_NDi_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_NDi_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_NDi_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.LocalAuthorityDestinations?.Apprentice_EAL_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_EAL_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_EAL_LA_Previous2_Pct),
                    _ => (x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Employment))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.LocalAuthorityDestinations?.Employment_Boy_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_Boy_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_Boy_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.LocalAuthorityDestinations?.Employment_Grl_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_Grl_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_Grl_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.LocalAuthorityDestinations?.Employment_Dis_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_Dis_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_Dis_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.LocalAuthorityDestinations?.Employment_NDi_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_NDi_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_NDi_LA_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.LocalAuthorityDestinations?.Employment_EAL_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_EAL_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_EAL_LA_Previous2_Pct),
                    _ => (x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct)
                };
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.LocalAuthorityDestinations?.AllDest_Boy_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Boy_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Boy_LA_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.LocalAuthorityDestinations?.AllDest_Grl_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Grl_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Grl_LA_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.LocalAuthorityDestinations?.AllDest_Dis_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Dis_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Dis_LA_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.LocalAuthorityDestinations?.AllDest_NDi_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_NDi_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_NDi_LA_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.LocalAuthorityDestinations?.AllDest_EAL_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_EAL_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_EAL_LA_Previous2_Pct),
                _ => (x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct, x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct)
            };
        }

        private static (
            Func<Ks4DestinationsData?, string?> Current,
            Func<Ks4DestinationsData?, string?> Previous,
            Func<Ks4DestinationsData?, string?> Previous2) ResolveEnglandAccessors(string destination, string characteristic)
        {
            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Education))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EnglandDestinations?.Education_Boy_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_Boy_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_Boy_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EnglandDestinations?.Education_Grl_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_Grl_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_Grl_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EnglandDestinations?.Education_Dis_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_Dis_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_Dis_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EnglandDestinations?.Education_NDi_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_NDi_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_NDi_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EnglandDestinations?.Education_EAL_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_EAL_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_EAL_Eng_Previous2_Pct),
                    _ => (x => x?.EnglandDestinations?.Education_Tot_Eng_Current_Pct, x => x?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct, x => x?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Apprenticeships))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EnglandDestinations?.Apprentice_Boy_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_Boy_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_Boy_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EnglandDestinations?.Apprentice_Grl_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_Grl_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_Grl_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EnglandDestinations?.Apprentice_Dis_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_Dis_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_Dis_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EnglandDestinations?.Apprentice_NDi_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_NDi_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_NDi_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EnglandDestinations?.Apprentice_EAL_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_EAL_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_EAL_Eng_Previous2_Pct),
                    _ => (x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Current_Pct, x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Previous_Pct, x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Previous2_Pct)
                };
            }

            if (destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Employment))
            {
                return characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EnglandDestinations?.Employment_Boy_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_Boy_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_Boy_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EnglandDestinations?.Employment_Grl_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_Grl_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_Grl_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EnglandDestinations?.Employment_Dis_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_Dis_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_Dis_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EnglandDestinations?.Employment_NDi_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_NDi_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_NDi_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EnglandDestinations?.Employment_EAL_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_EAL_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_EAL_Eng_Previous2_Pct),
                    _ => (x => x?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct, x => x?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct, x => x?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)
                };
            }

            return characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Boys) => (x => x?.EnglandDestinations?.AllDest_Boy_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_Boy_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_Boy_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Girls) => (x => x?.EnglandDestinations?.AllDest_Grl_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_Grl_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_Grl_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Disadvantaged) => (x => x?.EnglandDestinations?.AllDest_Dis_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_Dis_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_Dis_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (x => x?.EnglandDestinations?.AllDest_NDi_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_NDi_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_NDi_Eng_Previous2_Pct),
                _ when characteristic.EqualsCaseInsensitive(Ks4Destinations.Filters.PupilCharacteristic.Values.Eal) => (x => x?.EnglandDestinations?.AllDest_EAL_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_EAL_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_EAL_Eng_Previous2_Pct),
                _ => (x => x?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct, x => x?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct, x => x?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)
            };
        }
    }
}
