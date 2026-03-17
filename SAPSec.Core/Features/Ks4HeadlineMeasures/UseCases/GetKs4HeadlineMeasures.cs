using System.Globalization;
using SAPSec.Core;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetKs4HeadlineMeasures(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetKs4HeadlineMeasuresResponse> Execute(GetKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.TryGetByUrnAsync(request.Urn);
        if (schoolDetails is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        var data = await repository.GetByUrnAsync(request.Urn);

        var schoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num));

        var localAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num));

        var englandAverage = Average(
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));

        var schoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num));

        var localAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num));

        var englandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
            ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));

        var engMaths49SchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct));

        var engMaths49LocalAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct));

        var engMaths49EnglandAverage = Average(
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct));

        var engMaths49SchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct));

        var engMaths49LocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct));

        var engMaths49EnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct));

        var engMaths59SchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct));

        var engMaths59LocalAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct));

        var engMaths59EnglandAverage = Average(
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct));

        var engMaths59SchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct));

        var engMaths59LocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct));

        var engMaths59EnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct));

        var destinationsSchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct));

        var destinationsLocalAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct));

        var destinationsEnglandAverage = Average(
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct));

        var destinationsSchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct));

        var destinationsLocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct));

        var destinationsEnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct));

        var destinationsEducationSchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct));

        var destinationsEducationLocalAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct));

        var destinationsEducationEnglandAverage = Average(
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct));

        var destinationsEducationSchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct));

        var destinationsEducationLocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct));

        var destinationsEducationEnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct));

        var destinationsEmploymentSchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct));

        var destinationsEmploymentLocalAuthorityAverage = Average(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct));

        var destinationsEmploymentEnglandAverage = Average(
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct));

        var destinationsEmploymentSchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct));

        var destinationsEmploymentLocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
            ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct));

        var destinationsEmploymentEnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
            ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct));

        return new(
            schoolDetails,
            new Ks4HeadlineMeasureAverage(
                schoolAverage,
                localAuthorityAverage,
                englandAverage),
            new Ks4HeadlineMeasureYearByYear(
                schoolYearByYear,
                localAuthorityYearByYear,
                englandYearByYear),
            new Ks4HeadlineMeasureAverage(
                engMaths49SchoolAverage,
                engMaths49LocalAuthorityAverage,
                engMaths49EnglandAverage),
            new Ks4HeadlineMeasureYearByYear(
                engMaths49SchoolYearByYear,
                engMaths49LocalAuthorityYearByYear,
                engMaths49EnglandYearByYear),
            new Ks4HeadlineMeasureAverage(
                engMaths59SchoolAverage,
                engMaths59LocalAuthorityAverage,
                engMaths59EnglandAverage),
            new Ks4HeadlineMeasureYearByYear(
                engMaths59SchoolYearByYear,
                engMaths59LocalAuthorityYearByYear,
                engMaths59EnglandYearByYear),
            new Ks4HeadlineMeasureAverage(
                destinationsSchoolAverage,
                destinationsLocalAuthorityAverage,
                destinationsEnglandAverage),
            new Ks4HeadlineMeasureYearByYear(
                destinationsSchoolYearByYear,
                destinationsLocalAuthorityYearByYear,
                destinationsEnglandYearByYear),
            new Ks4HeadlineMeasureAverage(
                destinationsEducationSchoolAverage,
                destinationsEducationLocalAuthorityAverage,
                destinationsEducationEnglandAverage),
            new Ks4HeadlineMeasureYearByYear(
                destinationsEducationSchoolYearByYear,
                destinationsEducationLocalAuthorityYearByYear,
                destinationsEducationEnglandYearByYear),
            new Ks4HeadlineMeasureAverage(
                destinationsEmploymentSchoolAverage,
                destinationsEmploymentLocalAuthorityAverage,
                destinationsEmploymentEnglandAverage),
            new Ks4HeadlineMeasureYearByYear(
                destinationsEmploymentSchoolYearByYear,
                destinationsEmploymentLocalAuthorityYearByYear,
                destinationsEmploymentEnglandYearByYear));
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal? ParseNullableDecimal(double? value) =>
        !value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value)
            ? null
            : Convert.ToDecimal(value.Value);

    private static decimal? Average(params decimal?[] values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
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


