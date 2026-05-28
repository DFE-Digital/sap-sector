using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.Measures;

internal static class Ks4HeadlineMeasures
{
    public static class Attainment8
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSchool(
                "attainment8",
                "Attainment 8",
                MeasureDataType.Score,
                [],
                schoolData,
                similarSchools,
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return Measure.ForSchoolComparison(
                "attainment8",
                "Attainment 8",
                MeasureDataType.Score,
                [],
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData.Values,
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                    x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));
        }
    }

    public static class EnglishAndMaths
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            var engMathsGrade = filters.ContainsKey("eng-maths:grade") ? filters["eng-maths:grade"] : "4";

            return Measure.ForSchool(
                "eng-maths",
                "Grade achieved in English and maths GCSEs",
                MeasureDataType.GradePercentage,
                [
                    new MeasureAvailableFilter(
                    "eng-maths:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, engMathsGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, engMathsGrade == "5")
                    ]),
                ],
                schoolData,
                similarSchools,
                engMathsGrade switch
                {
                    "5" => new(
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct),
                    _ => new(
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)
                });
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            var engMathsGrade = filters.ContainsKey("eng-maths:grade") ? filters["eng-maths:grade"] : "4";

            return Measure.ForSchoolComparison(
                "eng-maths",
                "Grade achieved in English and maths GCSEs",
                MeasureDataType.GradePercentage,
                [
                    new MeasureAvailableFilter(
                    "eng-maths:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, engMathsGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, engMathsGrade == "5")
                    ]),
                ],
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData.Values,
                engMathsGrade switch
                {
                    "5" => new(
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct),
                    _ => new(
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                        x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct,
                        x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct,
                        x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)
                });
        }
    }

    public static class Destinations
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            var destinationType = filters.ContainsKey("destinations:destination") ? filters["destinations:destination"] : "all";

            return Measure.ForSchool(
                "destinations",
                "Staying in education or entering employment",
                MeasureDataType.GradePercentage,
                [
                    new MeasureAvailableFilter(
                    "destinations:destination",
                    "Destination", [
                        new FilterOption("all", "All destinations", 0, destinationType == "all"),
                        new FilterOption("education", "Education", 0, destinationType == "education"),
                        new FilterOption("employment", "Employment and apprenticeships", 0, destinationType == "employment")
                    ])
                ],
                schoolData,
                similarSchools,
                destinationType switch
                {
                    "education" => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),
                    "employment" => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct),
                    _ => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)
                });
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            var destinationType = filters.ContainsKey("destinations:destination") ? filters["destinations:destination"] : "all";

            return Measure.ForSchoolComparison(
                "destinations",
                "Staying in education or entering employment",
                MeasureDataType.GradePercentage,
                [
                    new MeasureAvailableFilter(
                    "destinations:destination",
                    "Destination", [
                        new FilterOption("all", "All destinations", 0, destinationType == "all"),
                        new FilterOption("education", "Education", 0, destinationType == "education"),
                        new FilterOption("employment", "Employment and apprenticeships", 0, destinationType == "employment")
                    ])
                ],
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData.Values,
                destinationType switch
                {
                    "education" => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),
                    "employment" => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct),
                    _ => new(
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                        x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct,
                        x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct,
                        x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)
                });
        }
    }
}
