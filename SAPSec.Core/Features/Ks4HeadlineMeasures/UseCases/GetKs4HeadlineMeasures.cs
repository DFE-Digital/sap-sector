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

        return new(
            schoolDetails,
            new Ks4HeadlineMeasureAverage(
                schoolAverage,
                localAuthorityAverage,
                englandAverage));
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

public record GetKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    Ks4HeadlineMeasureAverage Attainment8ThreeYearAverage);
