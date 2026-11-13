namespace SAPSec.Core.Features.SchoolSearch;

/// <summary>
/// Words that appear in most establishment names. A query made only of these (for example "school")
/// matches nearly every row, which SAP Public found too expensive under load testing, so such queries
/// are capped (see PostgresSchoolSearchIndexReader). Partial words typed in the typeahead ("sch") are
/// not affected: only a fully typed broad term triggers the cap.
/// </summary>
public static class BroadSearchTerms
{
    public const int BroadQueryLimit = 100;

    private static readonly HashSet<string> Terms = new(StringComparer.Ordinal)
    {
        "school", "schools", "academy", "academies", "primary", "secondary", "high", "college", "the", "of", "and"
    };

    public static bool IsBroadQuery(IReadOnlyList<string> tokens)
        => tokens.Count > 0 && tokens.All(Terms.Contains);
}
