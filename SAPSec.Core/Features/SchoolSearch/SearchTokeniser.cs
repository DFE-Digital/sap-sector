using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SchoolSearch;

public static class SearchTokeniser
{
    private static readonly Regex NonAlphanumeric = new(@"[^\p{L}\p{N}]+", RegexOptions.Compiled);

    /// <summary>Lower-cases and splits on any non letter/digit character, so output is safe to use in a tsquery.</summary>
    public static IReadOnlyList<string> Tokenise(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return NonAlphanumeric
            .Split(text.ToLowerInvariant())
            .Where(t => t.Length > 0)
            .ToList();
    }
}
