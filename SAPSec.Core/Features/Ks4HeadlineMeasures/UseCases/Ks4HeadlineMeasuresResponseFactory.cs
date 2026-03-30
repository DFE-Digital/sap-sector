using SAPSec.Core.Model;
using System.Globalization;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

internal static class Ks4HeadlineMeasuresResponseFactory
{
    public static GetKs4HeadlineMeasuresResponse Create(
        SchoolDetails schoolDetails,
        Ks4HeadlineMeasuresData? data) =>
        new(
            schoolDetails,
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                ParseNullableDecimal(data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                ParseNullableDecimal(data?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            BuildAverage(
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)),
            BuildYearByYear(
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                ParseNullableDecimal(data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                ParseNullableDecimal(data?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                ParseNullableDecimal(data?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)));

    private static Ks4HeadlineMeasureAverage BuildAverage(
        decimal? schoolCurrent,
        decimal? schoolPrevious,
        decimal? schoolPrevious2,
        decimal? localAuthorityCurrent,
        decimal? localAuthorityPrevious,
        decimal? localAuthorityPrevious2,
        decimal? englandCurrent,
        decimal? englandPrevious,
        decimal? englandPrevious2) =>
        new(
            Average(schoolCurrent, schoolPrevious, schoolPrevious2),
            Average(localAuthorityCurrent, localAuthorityPrevious, localAuthorityPrevious2),
            Average(englandCurrent, englandPrevious, englandPrevious2));

    private static Ks4HeadlineMeasureYearByYear BuildYearByYear(
        decimal? schoolCurrent,
        decimal? schoolPrevious,
        decimal? schoolPrevious2,
        decimal? localAuthorityCurrent,
        decimal? localAuthorityPrevious,
        decimal? localAuthorityPrevious2,
        decimal? englandCurrent,
        decimal? englandPrevious,
        decimal? englandPrevious2) =>
        new(
            new Ks4HeadlineMeasureSeries(schoolCurrent, schoolPrevious, schoolPrevious2),
            new Ks4HeadlineMeasureSeries(localAuthorityCurrent, localAuthorityPrevious, localAuthorityPrevious2),
            new Ks4HeadlineMeasureSeries(englandCurrent, englandPrevious, englandPrevious2));

    internal static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    internal static decimal? Average(params decimal?[] values)
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
