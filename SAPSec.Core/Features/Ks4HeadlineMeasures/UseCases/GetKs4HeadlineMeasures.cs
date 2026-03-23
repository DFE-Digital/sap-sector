using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetKs4HeadlineMeasures(
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetKs4HeadlineMeasuresResponse> Execute(GetKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var performanceData = await performanceRepository.GetByUrnAsync(request.Urn);
        var destinationsData = await destinationsRepository.GetByUrnAsync(request.Urn);

        return new GetKs4HeadlineMeasuresResponse(
            schoolDetails,
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)));
    }
}

public record GetKs4HeadlineMeasuresRequest(string Urn);

public record Ks4HeadlineMeasureAverage(
    decimal? SchoolValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record Ks4HeadlineMeasureSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record Ks4HeadlineMeasureYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

public record GetKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    Ks4HeadlineMeasureAverage Attainment8ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear Attainment8YearByYear,
    Ks4HeadlineMeasureAverage EngMaths49ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear EngMaths49YearByYear,
    Ks4HeadlineMeasureAverage EngMaths59ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear EngMaths59YearByYear,
    Ks4HeadlineMeasureAverage DestinationsThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsYearByYear,
    Ks4HeadlineMeasureAverage DestinationsEducationThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsEducationYearByYear,
    Ks4HeadlineMeasureAverage DestinationsEmploymentThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsEmploymentYearByYear);


