using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class StringExtensions
{
    public static string SplitPascalCase(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return source;
        }

        var stringBuilder = new StringBuilder(source.Length + 10);
        for (var index = 0; index < source.Length; ++index)
        {
            var c = source[index];

            if (char.IsUpper(c) && ((index > 1 && !char.IsUpper(source[index - 1])) || (index + 1 < source.Length && !char.IsUpper(source[index + 1]))))
            {
                stringBuilder.Append(' ');
            }

            if (index > 1
                && stringBuilder[Math.Min(stringBuilder.Length - 1, index + 1)] == ' '
                && !char.IsUpper(source[Math.Min(source.Length - 1, index + 1)]))
            {
                stringBuilder.Append(char.ToLower(c));
            }
            else
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Trim();
    }
}