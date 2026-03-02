using System.Globalization;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetKs4HeadlineMeasures(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetKs4HeadlineMeasuresResponse?> Execute(GetKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.TryGetByUrnAsync(request.Urn);
        if (schoolDetails is null)
        {
            return null;
        }

        var data = await repository.GetByUrnAsync(request.Urn);

        var schoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num));

        var localAuthorityAverage = Average(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num));

        var englandAverage = Average(
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));

        var schoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
            ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num));

        var localAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num));

        var englandYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
            ConvertNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));

        var engMaths49SchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct));

        var engMaths49LocalAuthorityAverage = Average(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct));

        var engMaths49EnglandAverage = Average(
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct));

        var engMaths49SchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct));

        var engMaths49LocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct));

        var engMaths49EnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct));

        var engMaths59SchoolAverage = Average(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct));

        var engMaths59LocalAuthorityAverage = Average(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct));

        var engMaths59EnglandAverage = Average(
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct));

        var engMaths59SchoolYearByYear = new Ks4HeadlineMeasureSeries(
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
            ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct));

        var engMaths59LocalAuthorityYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
            ConvertNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct));

        var engMaths59EnglandYearByYear = new Ks4HeadlineMeasureSeries(
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
            ConvertNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct));

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
            data?.EstablishmentTotalPupils,
            data?.EnglandTotalPupils);
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

    private static decimal? ConvertNullableDecimal(double? value) =>
        value is null ? null : Convert.ToDecimal(value.Value);

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
    int? SchoolTotalPupils,
    int? EnglandTotalPupils);
