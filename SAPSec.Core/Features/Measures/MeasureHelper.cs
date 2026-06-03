using System.Globalization;

namespace SAPSec.Core.Features.Measures;

public class MeasureHelper
{
    public static decimal? AverageFrom(IEnumerable<string?> stringValues) =>
        Average(stringValues.Select(ParseNullableDecimal));

    public static decimal? AverageFrom(params string?[] values) => AverageFrom((IEnumerable<string?>)values);

    public static YearByYearSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            ParseNullableDecimal(current),
            ParseNullableDecimal(previous),
            ParseNullableDecimal(previous2));

    public static decimal? Average(IEnumerable<decimal?> values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
    }

    public static decimal? Average(params decimal?[] values) => Average((IEnumerable<decimal?>)values);

    public static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}