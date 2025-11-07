using System.Text.RegularExpressions;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Core.Services.Lucene;

public class AbbreviationExpander : IAbbreviationExpander
{
    //todo: move this to somewhere else
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["st"] = "saint",
        ["cofe"] = "church of england",
        ["rd"] = "road",
        ["rm"] = "roman catholic",
        ["ave"] = "avenue",
        ["ave."] = "avenue",
        ["aven"] = "avenue",
        ["ln"] = "lane",
        ["dr"] = "drive",
        ["mt"] = "mount",
        ["ct"] = "court",
        ["pl"] = "place",
        ["blvd"] = "boulevard",
        ["sq"] = "square"
    };

    public string ExpandTerms(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText)) return string.Empty;

        // Build a final phrase that contains both the abbreviation and full expansion whenever either appears in the input.
        var finalText = queryText;

        var seenPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in Map)
        {
            var abbr = kv.Key;
            var full = kv.Value;
            var pairKey = $"{abbr}|{full}";

            if (!seenPairs.Add(pairKey))
                continue;

            var abbrPattern = Regex.Escape(abbr);
            var fullPattern = Regex.Escape(full);

            var hasAbbr = Regex.IsMatch(queryText, abbrPattern, RegexOptions.IgnoreCase);
            var hasFull = Regex.IsMatch(queryText, fullPattern, RegexOptions.IgnoreCase);

            if (hasAbbr && !hasFull)
            {
                // Insert the full form next to the abbreviation
                finalText = Regex.Replace(finalText, abbrPattern, $"{abbr} {full}", RegexOptions.IgnoreCase);
            }
            else if (hasFull && !hasAbbr)
            {
                // Insert the abbreviation next to the full form
                finalText = Regex.Replace(finalText, fullPattern, $"{full} {abbr}", RegexOptions.IgnoreCase);
            }
        }

        return finalText;
    }
}