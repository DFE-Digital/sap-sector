using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetKs4HeadlineMeasures(
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetKs4HeadlineMeasuresResponse> Execute(GetKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var data = await repository.GetByUrnAsync(request.Urn);

        return new GetKs4HeadlineMeasuresResponse(
            schoolDetails,
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)));
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


