using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Secondary;

internal static class Ks4PerformanceMeasures
{
    public static class Attainment8
    {
        public static Measure ForSecondarySchool(SchoolData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSecondarySchool(
                "attainment8",
                "Attainment 8",
                MeasureDataType.Score,
                [],
                currentSchool,
                similarSchools,
                new(
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));
        }

        public static Measure ForSecondarySchoolComparison(SchoolData<Ks4PerformanceData> currentSchool, SchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return Measure.ForSecondarySchoolComparison(
                "attainment8",
                "Attainment 8",
                MeasureDataType.Score,
                [],
                currentSchool,
                similarSchool,
                similarSchools,
                new(
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));
        }
    }

    public static class EnglishAndMaths
    {
        public static Measure ForSecondarySchool(SchoolData<Ks4PerformanceData> currentSchool, IEnumerable<SchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            var engMathsGrade = filters.ContainsKey("eng-maths:grade") ? filters["eng-maths:grade"] : "4";

            return Measure.ForSecondarySchool(
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
                currentSchool,
                similarSchools,
                engMathsGrade switch
                {
                    "5" => new(
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
                });
        }

        public static Measure ForSecondarySchoolComparison(SchoolData<Ks4PerformanceData> currentSchool, SchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            var engMathsGrade = filters.ContainsKey("eng-maths:grade") ? filters["eng-maths:grade"] : "4";

            return Measure.ForSecondarySchoolComparison(
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
                currentSchool,
                similarSchool,
                similarSchools,
                engMathsGrade switch
                {
                    "5" => new(
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
                });
        }
    }

    public static class Destinations
    {
        public static Measure ForSecondarySchool(SchoolData<Ks4DestinationsData> currentSchool, IEnumerable<SchoolData<Ks4DestinationsData>> similarSchools, IDictionary<string, string> filters)
        {
            var destinationType = filters.ContainsKey("destinations:destination") ? filters["destinations:destination"] : "all";

            return Measure.ForSecondarySchool(
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
                currentSchool,
                similarSchools,
                destinationType switch
                {
                    "education" => new(
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),
                    "employment" => new(
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
                });
        }

        public static Measure ForSecondarySchoolComparison(SchoolData<Ks4DestinationsData> currentSchool, SchoolData<Ks4DestinationsData> similarSchool, IEnumerable<SchoolData<Ks4DestinationsData>> similarSchools, IDictionary<string, string> filters)
        {
            var destinationType = filters.ContainsKey("destinations:destination") ? filters["destinations:destination"] : "all";

            return Measure.ForSecondarySchoolComparison(
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
                currentSchool,
                similarSchool,
                similarSchools,
                destinationType switch
                {
                    "education" => new(
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                        x => x?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                        x => x?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                        x => x?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),
                    "employment" => new(
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
                });
        }
    }
}
