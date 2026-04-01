using System.Globalization;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

internal static class Ks4HeadlineMeasuresCalculator
{
    internal static Ks4HeadlineMeasureAverage BuildAverage(
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

    internal static Ks4HeadlineMeasureYearByYear BuildYearByYear(
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
