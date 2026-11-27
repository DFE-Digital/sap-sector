namespace SAPSec.Core.Features.SchoolSearch;

/// <summary>Words found in most establishment names. A query made only of these is capped.</summary>
public static class BroadSearchTerms
{
    public const int BroadQueryLimit = 100;

    public static readonly HashSet<string> Terms = new(StringComparer.Ordinal)
    {
        "school", "schools", "academy", "academies", "primary", "secondary", "high", "college", "the", "of", "and"
    };

    public static bool IsBroadQuery(IReadOnlyList<string> tokens)
        => tokens.Count > 0 && tokens.All(Terms.Contains);
}
