namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// The establishment link types the predecessor/successor banner knows how to resolve into a
/// named, linkable successor school. This is the single mapping to extend when a new relationship
/// type is brought into scope - link types are never matched ad-hoc elsewhere.
/// </summary>
public static class SuccessorRelationshipTypeValues
{
    private static readonly HashSet<string> MappedLinkTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Successor",
        "Successor - amalgamated",
        "Result of Amalgamation"
    };

    public static bool IsMapped(string? linkType) =>
        !string.IsNullOrWhiteSpace(linkType) && MappedLinkTypes.Contains(linkType.Trim());
}
