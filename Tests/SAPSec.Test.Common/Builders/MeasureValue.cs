using System.Globalization;

namespace SAPSec.Test.Common.Builders;

/// <summary>
/// Test measure values are written as the text the source files contain ("42.7", "c", "").
/// The pipeline stores measures as numbers, turning anything that isn't a number into null (clean_numeric),
/// so builders convert with the same rule.
/// </summary>
public static class MeasureValue
{
    public static decimal? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value.EndsWith('%'))
            value = value[..^1];

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }
}
