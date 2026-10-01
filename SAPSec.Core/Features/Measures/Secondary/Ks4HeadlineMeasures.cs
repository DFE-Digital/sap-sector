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
            IEnumerable<MeasureAvailableFilter> availableFilters = [];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = new(
                x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num);

            return (availableFilters, fieldSelector);
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

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4Destinations.Filters.Destination.Key,
                    Ks4Destinations.Filters.Destination.Name,
                    Ks4Destinations.Filters.Destination.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(destination)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4DestinationsData> fieldSelector = destination switch
            {
                _ when destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Education) => new(
                    x => x?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                    x => x?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                    x => x?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                    x => x?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),

                _ when destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Apprenticeships) => new(
                    x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Current_Pct,
                    x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentDestinations?.Apprentice_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityDestinations?.Apprentice_Tot_LA_Previous2_Pct,
                    x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Current_Pct,
                    x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Previous_Pct,
                    x => x?.EnglandDestinations?.Apprentice_Tot_Eng_Previous2_Pct),

                _ when destination.EqualsCaseInsensitive(Ks4Destinations.Filters.Destination.Values.Employment) => new(
                    x => x?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x => x?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct,
                    x => x?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct,
                    x => x?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct,
                    x => x?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct,
                    x => x?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct,
                    x => x?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct,
                    x => x?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
        }
    }
}
