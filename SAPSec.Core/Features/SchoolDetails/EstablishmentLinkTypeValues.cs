namespace SAPSec.Core.Features.SchoolDetails;

public static class EstablishmentLinkTypeValues
{
    private const string SuccessorPrefix = "Successor";
    private const string PredecessorPrefix = "Predecessor";
    private const string ResultOfAmalgamation = "Result of Amalgamation";

    /// <summary>
    /// Broad check for "this link represents some kind of succession" - used to decide whether the
    /// plain closed-school banner should be suppressed. "Result of Amalgamation" is included because,
    /// even where the specific successor cannot be resolved (see <see cref="SuccessorRelationshipTypeValues"/>),
    /// the school is known to have a successor relationship of some kind.
    /// </summary>
    public static bool IsSuccessor(string? linkType) =>
        StartsWith(linkType, SuccessorPrefix) || Equals(linkType, ResultOfAmalgamation);

    public static bool IsPredecessor(string? linkType) =>
        StartsWith(linkType, PredecessorPrefix);

    private static bool StartsWith(string? linkType, string prefix) =>
        !string.IsNullOrWhiteSpace(linkType)
        && linkType.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    private static bool Equals(string? linkType, string value) =>
        !string.IsNullOrWhiteSpace(linkType)
        && string.Equals(linkType.Trim(), value, StringComparison.OrdinalIgnoreCase);
}
