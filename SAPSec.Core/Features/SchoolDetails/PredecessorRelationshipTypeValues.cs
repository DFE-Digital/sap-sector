namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// The establishment link types the successor banner (shown on a school that replaced an earlier,
/// closed school) knows how to resolve into a named, linkable predecessor school. This is the
/// single mapping to extend when a new relationship type is brought into scope - link types are
/// never matched ad-hoc elsewhere.
/// </summary>
public static class PredecessorRelationshipTypeValues
{
    private static readonly HashSet<string> MappedLinkTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Predecessor",
        "Predecessor - amalgamated",
        "Predecessor - merged"
    };

    public static bool IsMapped(string? linkType) =>
        !string.IsNullOrWhiteSpace(linkType) && MappedLinkTypes.Contains(linkType.Trim());
}
