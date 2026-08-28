using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Constants.Measures.Secondary;

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

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks4EnglishMaths.Filters.Grade.Key,
                    Ks4EnglishMaths.Filters.Grade.Name,
                    Ks4EnglishMaths.Filters.Grade.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(grade)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks4PerformanceData> fieldSelector = grade switch
            {
                _ when grade.EqualsCaseInsensitive(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove) => new(
                    x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct),

                _ => new(
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector);
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
                MeasureDataType.GradePercentage,
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
                MeasureDataType.GradePercentage,
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
