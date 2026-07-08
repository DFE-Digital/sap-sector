namespace SAPSec.Core.Features.Measures;

public record YearByYearSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2)
{
    public static YearByYearSeries FromStringValues(string? current, string? previous, string? previous2) =>
        new(
            MeasureHelper.ParseNullableDecimal(current),
            MeasureHelper.ParseNullableDecimal(previous),
            MeasureHelper.ParseNullableDecimal(previous2));
}