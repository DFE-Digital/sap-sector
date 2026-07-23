namespace SAPSec.Core.Extensions;

public static class StringExtensions
{
    public static bool EqualsCaseInsensitive(this string? text, string? other) =>
        string.Equals(text, other, StringComparison.InvariantCultureIgnoreCase);
}
