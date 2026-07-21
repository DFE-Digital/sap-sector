using System.Globalization;

namespace SAPSec.Core.Features.SimilarSchools;

internal static class SimilarSchoolsDecimalParsing
{
    public static decimal ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}
