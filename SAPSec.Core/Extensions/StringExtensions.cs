namespace SAPSec.Core.Extensions;

public static class StringExtensions
{
    public static bool EqualsCaseInsensitive(this string? text, string? other) =>
        string.Equals(text, other, StringComparison.InvariantCultureIgnoreCase);

    public static string SanitizeForLog(this string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}
